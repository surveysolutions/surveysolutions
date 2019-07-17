using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class PublisherToExternalStorage : IPublisherToExternalStorage
    {
        private readonly IExternalStorageDataClientFactory externalStorageDataClientFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportFileNameService exportFileNameService;
        private readonly ILogger<PublisherToExternalStorage> logger;

        public PublisherToExternalStorage(
            IExternalStorageDataClientFactory externalStorageDataClientFactory,
            IQuestionnaireStorage questionnaireStorage,
            IExportFileNameService exportFileNameService,
            ILogger<PublisherToExternalStorage> logger)
        {
            this.externalStorageDataClientFactory = externalStorageDataClientFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.exportFileNameService = exportFileNameService;
            this.logger = logger;
        }

        public async Task PublishToExternalStorage(ExportState state, CancellationToken cancellationToken = default)
        {
            logger.LogTrace("Publishing to external storage. File: {archive}. {storageType}", 
                state.ArchiveFilePath, state.StorageType);

            var dataClient = externalStorageDataClientFactory.GetDataClient(state.StorageType);

            using (dataClient.InitializeDataClient(state.ProcessArgs.AccessToken, state.Settings.Tenant))
            {
                var applicationFolder = await dataClient.CreateApplicationFolderAsync("Data Export");

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(state.Settings.QuestionnaireId, cancellationToken);
                
                using (var fileStream = File.OpenRead(state.ArchiveFilePath))
                {
                    var filename = this.exportFileNameService.GetFileNameForExportArchive(state.Settings, questionnaire.VariableName);
                    await dataClient.UploadFileAsync(applicationFolder, filename, fileStream, fileStream.Length, cancellationToken);
                }
            }
        }
    }
}
