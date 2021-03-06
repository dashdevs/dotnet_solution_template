# DashDevs dotnet new template examples

## Solution template

The template can basically be divided into 2 parts: the configuration is located at [.template.config/template.json](solution/.template.config/template.json), and other files will be included in the resulting template as the subject of modification according to the configuration rules.

### Template configuration

Let's look at the sections which hold the most of our interest which actually do everything we need.

`"shortName": "dashdevs_sln_standard"` - this is the name to be used as an alias for your template after installation, i.e. here it will be `dotnet new dashdevs_sln_standard`

In this template, we use the *TemplateCompany/templatecompany*, *TemplateProduct/templateproduct* and *TemplateService/templateservice* names for a company, a product and service, respectively. 
The next section will serve as an example of how we modify the default company name (*TemplateCompany/templatecompany*).

```json
"symbols": {
    ...
    "company": {
        "type": "parameter",
        "datatype": "string",
        "defaultValue": "TemplateCompany",
        "replaces": "TemplateCompany",
        "fileRename": "TemplateCompany"
    },
    "companyLowerCase": {
       "type": "generated",
       "generator": "casing",
       "parameters": {
         "source": "company",
         "toLower": true
       },
       "replaces": "templatecompany",
       "fileRename": "templatecompany"
    }
    ...
```

Here we first introduce the template string parameter symbol named *company*, with default value *TemplateCompany*.
*replaces* field instructs the engine to replace all text occurrences of the value, which is *TemplateCompany* (it's case-sensitive).
*fileRename* field uses the similar logic, but for files and folder names instead of file contents modification.
Thus, if the parameter is not set, nothing changes, because we use *TemplateCompany* across the template files already.

Then we process the lowercase occurences of the *company* parameter by using a *generated* type of new *companyLowerCase* symbol and specifying the *casing* type of the *generator*.
Generated symbols are using predefined value as a source, so we specify the *company* parameter value as the one and set *toLower* field to *true*, because we want to generate the lowercase string.
*replaces* and *fileName* fields logic was described above.

The next section allows to define the options for template sources.

```json
"sources": [
  {
    "exclude": [ "**/[Bb]in/**", "**/[Oo]bj/**", ".template.config/**/*", "**/*.filelist", "**/*.user", "**/*.lock.json", ".git/**", ".vs/**", "_ReSharper*/", "*.[Rr]e[Ss]harper" ]
  }
]
```

Here we use only *exclude* option, which can be easily modified according to your needs.

### Template installation and usage

To install the template locally from the folder, please run `dotnet new -i pathToTemplateFolder`.
Once it's done, you will see the list of all templates, including the new one.

To use the template, please run `dotnet new templateShortName --company YourCompanyName --product YourProductName --service YourServiceName`.
*templateShortName* is the value of *shortName* field; *product*, *company* and *service* are parameter symbols from [.template.config/template.json](solution/.template.config/template.json) (*company* example is described above).

Thus, the current example would require running `dotnet new dashdevs_sln_standard --company YourCompanyName --product YourProductName --service YourServiceName`.

### [Tools folder](solution/tools)

This folder contains tools to be used for service templates post-processing, which will be revealed below.

## Service template

The particular template is used in connjuction with the solution template to add new services for existing solutions.

### Template configuration

Generally they're very similar, though service introduces some important additions.

The first one is *choice* parameter type, where we can specify the list of available params. Here it's used to distinguish the OS, which is required for specific post-processing tasks, which will be described later.

```json
"symbols": {
    ...
    "OS": {
       "type":"parameter",
       "datatype": "choice",
       "defaultValue":"nix",
       "choices": [
         {
           "choice": "win"
         },
         {
           "choice": "nix"
         }
       ]
    }
    ...
```

The second one is using a concept of [*post-actions*](https://github.com/dotnet/templating/wiki/Post-Action-Registry) to be executed after main template engine actions are finished. Our interest lies in running external scripts by specifying the *actionId* value "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2" and other parameters.

The reason to use custom scripts is quite trivial: unfortunately, [dotnet template engine can't modify existing files](https://github.com/dotnet/templating/issues/2148). Here you can see how the action for adding newly created service from a template to solution is defined. 

```json
"postActions": [
   ...
   {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "condition": "(OS == \"win\")",
    "args": {
      "executable": "./tools/add_projects.bat",
      "args": ""
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "condition": "(OS == \"nix\")",
    "args": {
      "executable": "./tools/add_projects.sh",
      "args": ""
    },
    "continueOnError": false
  },
  ...
]
```

Depending on the *OS* parameter, either add_projects.bat or add_projects.sh is called. Please, note that these files are located in the [solution template](solution/tools), because they're not service-specific and depend on a solution. Also, they actually search for all projects in the *./src/services* folder to add.

### Problems of adding a new service

As you can see, the particular problem with adding projects to a solution can be solved quite simply by means of dotnet CLI, but what about other use-cases? Many of them imply absence of a ready-to-use tool, and because of inability of existing files modification the only option that's left is making a custom one for each use-case.

For example, one of the most used templates would be *docker-compose* configuration modification. For this, we've implemented custom [template post-processor](solution/tools/DashDevs.TemplatePostProcessor), which actually does the following:

1. Accepts custom template path (we put all of them in service's [*postprocessing*](service/postprocessing) folder. This is a text file which is a subject of template source files modification like any other, so the in-text replacement is made by the engine, thus we read the file which is already modified.
2. Accepts path to the existing file to be modified (*docker-compose YAML* in our case) and reads it.
3. Searches for a place to insert text from a template and inserts it.

This tool is simple and extandable, thus it's easy to add new post-processors. Let's look how it correlates with [.template.config/template.json](service/.template.config/template.json):

```json
"postActions": [
  ...
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "args": {
      "executable": "dotnet",
      "args": "publish ./tools/DashDevs.TemplatePostProcessor/DashDevs.TemplatePostProcessor.csproj -o ./tools/DashDevs.TemplatePostProcessor/publish"
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "args": {
      "executable": "dotnet",
      "args": "./tools/DashDevs.TemplatePostProcessor/publish/DashDevs.TemplatePostProcessor.dll --docker-compose ./postprocessing/docker-compose/docker-compose.txt ./docker-compose.yml"
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "args": {
      "executable": "dotnet",
      "args": "./tools/DashDevs.TemplatePostProcessor/publish/DashDevs.TemplatePostProcessor.dll --docker-compose ./postprocessing/docker-compose/docker-compose.Development.txt ./docker-compose.Development.yml"
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "args": {
      "executable": "dotnet",
      "args": "./tools/DashDevs.TemplatePostProcessor/publish/DashDevs.TemplatePostProcessor.dll --docker-compose ./postprocessing/docker-compose/docker-compose.Production.txt ./docker-compose.Production.yml"
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "condition": "(OS == \"nix\")",
    "args": {
      "executable": "rm",
      "args": "-drf ./postprocessing"
    },
    "continueOnError": false
  },
  {
    "actionId": "3A7C4B45-1F5D-4A30-959A-51B88E82B5D2",
    "condition": "(OS == \"win\")",
    "args": {
      "executable": "powershell",
      "args": "-command Remove-Item -Recurse -Force \"postprocessing\""
    },
    "continueOnError": false
  }
]
```

The flow is next:

1. Publishing the template post-processor.
2. Applying the templates to corresponding existing files.
3. Cleanup. By the way, tool or script execution context [may get broken for some reason](https://github.com/dotnet/templating/issues/1663#issuecomment-612181514), so in Windows environment we had to use Powershell workaround not to make additional scripts.  

### Template installation and usage

The template can be installed just the same as the solution one, but as we've reviewed two templates already, there's a tip: you can run `dotnet new -i pathToRootTemplateFolder` within the root folder and both of them will be installed at once.

To use the template, please run `dotnet new templateShortName --company YourCompanyName --product YourProductName --service YourServiceName --OS yourOS`. Another tip is to add `--allow-scripts yes` for templates which use scripts in post-actions to prevent the engine from asking your confirmation before each separate script is run.

## Customization

Given the current information, you can easily add or remove symbol parameters, extend the post-processing, or exclude additional unwanted sources according to specific needs of your own solution.

## NuGet

In a real world, you will probably use your templates as a package. This is an easy thing to do and is described in the [official docs](https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-pack).
