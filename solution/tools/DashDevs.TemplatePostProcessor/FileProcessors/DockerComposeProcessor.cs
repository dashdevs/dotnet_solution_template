using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashDevs.TemplatePostProcessor.FileProcessors
{
    internal class DockerComposeProcessor : IFileProcessor
    {
        private const string DefaultTab = "  ";
        private const string ServicesToken = "services:";

        public async Task Process(string templatePath, string existingFilePath)
    {
            var templateStrings = await File.ReadAllLinesAsync(templatePath);
            IEnumerable<string> linesToInsert = templateStrings;

            if (!templateStrings.First().StartsWith(DefaultTab))
            {
                linesToInsert = templateStrings.Select(s => s.Insert(0, DefaultTab));
            }

            var composeFile = await File.ReadAllLinesAsync(existingFilePath);
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
                    .FirstOrDefault(s => !s.StartsWith(DefaultTab) && composeFileList.IndexOf(s) > servicesLineIndex);

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

            var sb = new StringBuilder();
            composeFileList.ForEach(l => sb.AppendLine(l));
            await File.WriteAllTextAsync(existingFilePath, sb.ToString());
        }
    }
}
