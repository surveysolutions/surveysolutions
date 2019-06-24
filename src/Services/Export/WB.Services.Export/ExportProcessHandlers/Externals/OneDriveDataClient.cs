using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OneDriveDataClient : IExternalDataClient
    {
        private readonly ILogger<OneDriveDataClient> logger;
        private IGraphServiceClient graphServiceClient;

        private static long MaxAllowedFileSizeByMicrosoftGraphApi = 4 * 1024 * 1024;

        public OneDriveDataClient(
            ILogger<OneDriveDataClient> logger)
        {
            this.logger = logger;
        }

        public IDisposable GetClient(string accessToken)
        {
            logger.LogTrace("Creating Microsoft.Graph.Client for OneDrive file upload");

            graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.CompletedTask;
            }));

            return null;
        }

        public Task<string> CreateApplicationFolderAsync() => Task.FromResult("Survey Solutions");

        public Task<string> CreateFolderAsync(string applicationFolder, string folderName)
            => Task.FromResult($"{applicationFolder}/{folderName}");

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            if (fileStream.Length > MaxAllowedFileSizeByMicrosoftGraphApi)
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Large file of size {Length} in chunks", 
                    fileName, folder, contentLength);
                const int maxSizeChunk = 320 * 4 * 1024;
                
                var item = graphServiceClient.Drive.Root.ItemWithPath($"{folder}/{fileName}");
                var session = await item.CreateUploadSession().Request().PostAsync(cancellationToken);
                var chunkUploader = new ChunkedUploadProvider(session, graphServiceClient, fileStream, maxSizeChunk);
                await chunkUploader.UploadAsync();
            
            }
            else
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Small file of size {Length}", fileName, folder, fileStream.Length);
                var item = graphServiceClient.Drive.Root.ItemWithPath($"{folder}/{fileName}");
                await item.Content.Request().PutAsync<DriveItem>(fileStream);
            }
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            var storageInfo = await graphServiceClient.Drive.Request().GetAsync();
            if (storageInfo?.Quota?.Total == null) return null;

            return storageInfo.Quota.Total - storageInfo.Quota.Used ?? 0;
        }
    }
}
