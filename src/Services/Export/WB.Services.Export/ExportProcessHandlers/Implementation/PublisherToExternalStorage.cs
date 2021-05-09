using System;
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

            if (dataClient == null)
                throw new ArgumentException("Cannot find appropriate external storage data client for type: " + state.StorageType);
            if (state.ProcessArgs.AccessToken == null)
                throw new ArgumentException("Cannot publish to external storage export without accessToken", nameof(ExportState.ProcessArgs.AccessToken));
            if (state.ProcessArgs.RefreshToken == null)
                throw new ArgumentException("Cannot publish to external storage export without refreshToken", nameof(ExportState.ProcessArgs.RefreshToken));

            dataClient.InitializeDataClient(state.ProcessArgs.AccessToken, state.ProcessArgs.RefreshToken,
                state.Settings.Tenant);
            
            using (dataClient)
            {
                var applicationFolder = await dataClient.CreateApplicationFolderAsync("Data Export");

                var questionnaire = await questionnaireStorage.GetQuestionnaireAsync(state.Settings.QuestionnaireId, 
                    token: cancellationToken);

                if (questionnaire == null)
                    throw new InvalidOperationException("Questionnaire was not found.");

                await using (var fileStream = File.OpenRead(state.ArchiveFilePath))
                {
                    string? questionnaireNamePrefixOverride = null;
                    if (!string.IsNullOrEmpty(questionnaire.VariableName))
                    {
                        var questionnaireVersion = questionnaire.QuestionnaireId.Version;
                        questionnaireNamePrefixOverride = $"{questionnaire.VariableName}_v{questionnaireVersion}";
                    }
                    var filename = await this.exportFileNameService.GetFileNameForExportArchiveAsync(state.Settings, questionnaireNamePrefixOverride);
                    await dataClient.UploadFileAsync(applicationFolder, filename, fileStream, fileStream.Length, cancellationToken);
                }
            }
        }
    }
}
