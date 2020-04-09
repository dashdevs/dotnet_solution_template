using DashDevs.TemplatePostProcessor.CommandHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DashDevs.TemplatePostProcessor.FileProcessors
{
    internal class DockerComposeProcessor
    {
        private const string DefaultTab = "  ";
        private const string ServicesToken = "services:";
        private readonly BaseFileProcessor _baseFileProcessor = new BaseFileProcessor();

        public async Task AddService(string templatePath, Dictionary<string, string> replaceMap, string dockerComposePath)
        {
            var templateStrings = await _baseFileProcessor.ReadFileTemplateAsStrings(templatePath, replaceMap);
            IEnumerable<string> linesToInsert = templateStrings;

            if (!templateStrings.First().StartsWith(DefaultTab))
            {
                linesToInsert = templateStrings.Select(s => s.Insert(0, DefaultTab));
            }

            var composeFile = await File.ReadAllLinesAsync(dockerComposePath);
            var composeFileList = composeFile.ToList();
            string actualServicesString = composeFileList.FirstOrDefault(s => s.Contains(ServicesToken));

            var newLine = Environment.NewLine;

            int insertionIndex;
            if (string.IsNullOrWhiteSpace(actualServicesString))
            {
                composeFileList.Add($"{newLine}{ServicesToken}");
                insertionIndex = composeFileList.Count;
            }
            else
            {
                var servicesLineIndex = composeFileList.IndexOf(actualServicesString);
                var newSectionString = composeFileList
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s) && composeFileList.IndexOf(s) > servicesLineIndex);

                if (!string.IsNullOrEmpty(newSectionString))
                {
                    insertionIndex = composeFileList.IndexOf(newSectionString);
                }
                else
                {
                    insertionIndex = composeFileList.Count;
                }
            }

            composeFileList.InsertRange(insertionIndex, linesToInsert);
        }
    }
}
