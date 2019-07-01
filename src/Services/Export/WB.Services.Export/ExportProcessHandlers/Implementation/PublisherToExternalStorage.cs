using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    class PublisherToExternalStorage : IPublisherToExternalStorage
    {
        private readonly IExternalStorageDataClientFactory externalStorageDataClientFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IExportFileNameService exportFileNameService;

        public PublisherToExternalStorage(
            IExternalStorageDataClientFactory externalStorageDataClientFactory,
            IQuestionnaireStorage questionnaireStorage,
            IExportFileNameService exportFileNameService)
        {
            this.externalStorageDataClientFactory = externalStorageDataClientFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.exportFileNameService = exportFileNameService;
        }

        public async Task PublishToExternalStorage(ExportState state, CancellationToken cancellationToken = default)
        {
            var dataClient = externalStorageDataClientFactory.GetDataClient(state.StorageType);

            using (dataClient.GetClient(state.ProcessArgs.AccessToken))
            {
                var applicationFolder = await dataClient.CreateApplicationFolderAsync();

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(state.Settings.QuestionnaireId, cancellationToken);
                var questionnaireFolder = "Data Export";

                var folder = await dataClient.CreateFolderAsync(applicationFolder, questionnaireFolder);

                using (var fileStream = File.OpenRead(state.ArchiveFilePath))
                {
                    var filename = this.exportFileNameService.GetFileNameForExportArchive(state.Settings, questionnaire.VariableName);
                    await dataClient.UploadFileAsync(folder, filename, fileStream, fileStream.Length, cancellationToken);
                }
            }
        }
    }
}
