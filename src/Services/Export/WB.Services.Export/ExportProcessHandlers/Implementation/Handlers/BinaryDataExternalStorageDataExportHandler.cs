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
            this.interviewDataExportSettings = interviewDataExportSettings ?? throw new ArgumentException("Export settings are missing.");
            this.binaryDataSource = binaryDataSource;
            this.externalDataClientFactory = externalDataClientFactory;
        }

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            using (var dataClient = externalDataClientFactory.GetDataClient(state.StorageType))
            {
                if (dataClient == null)
                    throw new ArgumentException("Cannot find appropriate external storage data client for type: " + state.StorageType);
                
                if (state.ProcessArgs.AccessToken == null)
                    throw new ArgumentException("Cannot start external storage export without accessToken",
                        nameof(ExportState.ProcessArgs.AccessToken));

                if (state.ProcessArgs.RefreshToken == null)
                    throw new ArgumentException("Cannot start external storage export without refreshToken",
                        nameof(ExportState.ProcessArgs.RefreshToken));

                dataClient.InitializeDataClient(state.ProcessArgs.AccessToken, state.ProcessArgs.RefreshToken,
                        state.Settings.Tenant);

                string applicationFolder = await dataClient.CreateApplicationFolderAsync("Binary Data");

                string GetInterviewFolder(string interviewId) => $"{state.QuestionnaireName}/{interviewId}";

                string GetAudioAuditInterviewFolder(string interviewId) =>
                    $"{GetInterviewFolder(interviewId)}/{interviewDataExportSettings.Value?.AudioAuditFolderName}";

                async Task<string> GetOrCreateFolderByType(BinaryDataType binaryDataType, string interviewId)
                {
                    if (dataClient == null)
                        throw new ArgumentException("Cannot find appropriate external storage data client for type: " + state.StorageType);

                    switch (binaryDataType)
                    {
                        case BinaryDataType.Audio:
                        case BinaryDataType.Image:
                            return await dataClient.CreateFolderAsync(GetInterviewFolder(interviewId!),
                                applicationFolder);
                        case BinaryDataType.AudioAudit:
                            return await dataClient.CreateFolderAsync(GetAudioAuditInterviewFolder(interviewId!),
                                applicationFolder);
                        default:
                            throw new ArgumentException("Unknown binary type: " + binaryDataType);
                    }
                }

                await binaryDataSource.ForEachInterviewMultimediaAsync(state,
                    async binaryDataAction =>
                    {
                        var folderPath = await GetOrCreateFolderByType(binaryDataAction.Type,
                            binaryDataAction.InterviewKey ?? binaryDataAction.InterviewId.FormatGuid());

                        var storageSize = await dataClient.GetFreeSpaceAsync(cancellationToken);
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
