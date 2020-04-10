using System.Threading.Tasks;

namespace DashDevs.TemplatePostProcessor.FileProcessors
{
    internal interface IFileProcessor
    {
        Task Process(string templatePath, string existingFilePath);
    }
}
