using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Infrastructure;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    internal class BinaryDataExternalStorageDataExportHandler : IExportHandler
    {
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IExternalStorageDataClientFactory externalDataClientFactory;
        private readonly IBinaryDataSource binaryDataSource;

        public BinaryDataExternalStorageDataExportHandler(
            IOptions<ExportServiceSettings> interviewDataExportSettings,
            IExternalStorageDataClientFactory externalDataClientFactory,
            IBinaryDataSource binaryDataSource)
            
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.binaryDataSource = binaryDataSource;
            this.externalDataClientFactory = externalDataClientFactory;
        }

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;

            var dataClient = externalDataClientFactory.GetDataClient(state.StorageType);

            if (dataClient == null)
            {
                throw new ArgumentException("Cannot find appropriate external storage data client for type: " + state.StorageType);
            }

            using (dataClient.GetClient(state.ProcessArgs.AccessToken))
            {
                var applicationFolder = await dataClient.CreateApplicationFolderAsync();

                string GetInterviewFolder(Guid interviewId) => $"{settings.QuestionnaireId}/{interviewId.FormatGuid()}";
                string GetAudioAuditInterviewFolder(Guid interviewId) => $"{GetInterviewFolder(interviewId)}/{interviewDataExportSettings.Value.AudioAuditFolderName}";

                async Task<string> GetOrCreateFolderByType(BinaryDataType binaryDataType, Guid interviewId)
                {
                    switch (binaryDataType)
                    {
                        case BinaryDataType.Audio:
                        case BinaryDataType.Image:
                            return await dataClient.CreateFolderAsync(applicationFolder, GetInterviewFolder(interviewId));
                        case BinaryDataType.AudioAudit:
                            return await dataClient.CreateFolderAsync(applicationFolder, GetAudioAuditInterviewFolder(interviewId));
                        default:
                            throw new ArgumentException("Unknown binary type: " + binaryDataType);
                    }
                }

                await binaryDataSource.ForEachInterviewMultimediaAsync(state,
                    async binaryDataAction =>
                    {
                        var folderPath = await GetOrCreateFolderByType(binaryDataAction.Type, binaryDataAction.InterviewId);

                        var storageSize = await dataClient.GetFreeSpaceAsync();
                        if (storageSize.HasValue && storageSize.Value < binaryDataAction.ContentLength)
                            throw new IOException("There is not enough space on the disk", 0x70);

                        await dataClient.UploadFileAsync(folderPath, binaryDataAction.FileName, 
                            binaryDataAction.Content, 
                            binaryDataAction.ContentLength, 
                            cancellationToken);
                    }, cancellationToken);
            }

            // this export type do not require any compression
            state.RequireCompression = false;

            // binary data is already in artifacts storage
            state.RequirePublishToArtifactStorage = false;

            // this export is already to publish to external storage
            state.RequirePublishToExternalStorage = false;
        }
    }
}
