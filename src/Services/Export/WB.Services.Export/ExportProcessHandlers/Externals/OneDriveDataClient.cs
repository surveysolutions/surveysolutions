using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Polly;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using File = System.IO.File;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class OneDriveDataClient : IExternalDataClient
    {
        private readonly ILogger<OneDriveDataClient> logger;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private IGraphServiceClient graphServiceClient;
        private TenantInfo tenant;
        private string refreshToken;

        private static long MaxAllowedFileSizeByMicrosoftGraphApi = 4 * 1024 * 1024;

        public OneDriveDataClient(
            ILogger<OneDriveDataClient> logger,
            ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.logger = logger;
            this.tenantApi = tenantApi;
        }

        public void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant)
        {
            this.tenant = tenant;
            this.refreshToken = refreshToken;

            this.CreateClient(accessToken);
        }

        private void CreateClient(string accessToken)
        {
            logger.LogTrace("Creating Microsoft.Graph.Client for OneDrive file upload");

            this.graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(requestMessage =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.CompletedTask;
            }));
        }

        private string Join(params string[] path) 
            => string.Join("/", path.Where( p => p != null));

        public Task<string> CreateApplicationFolderAsync(string subFolder)
            => Task.FromResult(Join("Survey Solutions", tenant.Name, subFolder));

        public Task<string> CreateFolderAsync(string folder, string parentFolder)
            => Task.FromResult(Join(parentFolder, folder));

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            if (contentLength > MaxAllowedFileSizeByMicrosoftGraphApi)
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Large file of size {Length} in chunks",
                    fileName, folder, contentLength);
                const int maxSizeChunk = 320 * 4 * 1024;

                var temp = Path.GetTempFileName();
                var fs = File.Open(temp, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    await fileStream.CopyToAsync(fs);

                    await this.ExecuteRequestAsync(async () =>
                    {
                        var session = await graphServiceClient.Drive.Root.ItemWithPath(Join(folder, fileName)).CreateUploadSession().Request()
                            .PostAsync(cancellationToken);
                        
                        return new ChunkedUploadProvider(session, graphServiceClient, fs, maxSizeChunk).UploadAsync();
                    });
                }
                finally
                {
                    fs.Close();
                    File.Delete(temp);
                }
            }
            else
            {
                logger.LogTrace("Uploading {fileName} to {folder}. Small file of size {Length}", fileName, folder, contentLength);
                
                await this.ExecuteRequestAsync(() =>
                    graphServiceClient.Drive.Root.ItemWithPath(Join(folder, fileName)).Content.Request()
                        .PutAsync<DriveItem>(fileStream));
            }
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            var storageInfo = await this.ExecuteRequestAsync(graphServiceClient.Drive.Request().GetAsync);
            if (storageInfo?.Quota?.Total == null) return null;

            return storageInfo.Quota.Total - storageInfo.Quota.Used ?? 0;
        }

        public async Task<T> ExecuteRequestAsync<T>(Func<Task<T>> request) =>
            await Policy.Handle<ServiceException>(e => e.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(2, async (exception, span) =>
                {
                    this.logger.LogError(exception, $"Unauthorized exception during request to OneDrive");

                    var newAccessToken = await this.tenantApi.For(this.tenant)
                        .GetExternalStorageAccessTokenByRefreshTokenAsync(ExternalStorageType.OneDrive,
                            this.refreshToken).ConfigureAwait(false);

                    this.CreateClient(newAccessToken);

                })
                .ExecuteAsync(async () => await request());

        public void Dispose() { }
    }
}
