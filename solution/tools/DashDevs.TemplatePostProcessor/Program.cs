using DashDevs.TemplatePostProcessor.FileProcessors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DashDevs.TemplatePostProcessor
{
    class Program
    {
        private const string CommandToken = "--";

        private static readonly Dictionary<string, IFileProcessor> _processors = new Dictionary<string, IFileProcessor>()
        {
            { $"{CommandToken}docker-compose", new DockerComposeProcessor() }
        };

        static async Task Main(string[] args)
        {
            if (args == null || args.Length != 3)
            {
                Console.WriteLine("There should be 3 arguments: command type, template path and existing file path to modify!");
                return;
            }

            if (!args[0].StartsWith(CommandToken))
            {
                Console.WriteLine("Please, specify the command type in a first parameter!");
                return;
            }

            try
            {
                await _processors[args[0]].Process(args[1], args[2]);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Template post-processing failed with exception!{Environment.NewLine}{ex}");
            }
        }
    }
}
