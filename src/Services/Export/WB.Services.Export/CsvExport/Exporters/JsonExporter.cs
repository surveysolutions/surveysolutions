using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IJsonExporter
    {
        Task ExportAsync(QuestionnaireDocument questionnaire, string basePath, CancellationToken cancellationToken);
    }

    public class JsonExporter : IJsonExporter
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger<JsonExporter> logger;
        public JsonExporter(IFileSystemAccessor fileSystemAccessor, ILogger<JsonExporter> logger)
        {
            this.fileSystemAccessor = fileSystemAccessor ?? throw new ArgumentNullException(nameof(fileSystemAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExportAsync(QuestionnaireDocument questionnaire, string basePath, 
            CancellationToken cancellationToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(
                    questionnaire, 
                    Formatting.Indented, 
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                var targetFileName = Path.ChangeExtension(questionnaire.VariableName, ".json");
                targetFileName = fileSystemAccessor.MakeValidFileName(targetFileName);
                var targetFolder = Path.Combine(basePath, "Questionnaire");
                var mainFilePath = Path.Combine(targetFolder, targetFileName);
                Directory.CreateDirectory(targetFolder);

                await fileSystemAccessor.WriteAllTextAsync(mainFilePath, json);
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, "Error on json export.", e);
                throw;
            }
        }
    }
}
