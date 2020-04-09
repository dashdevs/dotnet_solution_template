using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DashDevs.TemplatePostProcessor.CommandHandlers
{
    internal class BaseFileProcessor
    {
        public async Task<string> ReadFileTemplate(string path, Dictionary<string, string> replaceMap)
        {
            var fileContents = await File.ReadAllTextAsync(path);

            if (replaceMap != null)
            {
                foreach (var kvp in replaceMap)
                {
                    fileContents = fileContents.Replace(kvp.Key, kvp.Value);
                }
            }

            return fileContents;
        }

        public async Task<IEnumerable<string>> ReadFileTemplateAsStrings(string path, Dictionary<string, string> replaceMap)
        {
            var fileContents = await File.ReadAllLinesAsync(path);
            IEnumerable<string> stringList = fileContents;

            if (fileContents.Any() && replaceMap != null && replaceMap.Count > 0)
            {
                stringList = fileContents.Select(s =>
                {
                    var replacedString = s;
                    foreach (var kvp in replaceMap)
                    {
                        replacedString = replacedString.Replace(kvp.Key, kvp.Value);
                    }

                    return replacedString;
                });
            }

            return stringList;
        }

        public async Task AppendFileContents(string path, string contents, bool prependLine = true)
        {
            prependLine = prependLine && !contents.EndsWith(Environment.NewLine);

            await File.AppendAllTextAsync(path, $"{(prependLine ? Environment.NewLine : string.Empty)}{contents}");
        }
    }
}
