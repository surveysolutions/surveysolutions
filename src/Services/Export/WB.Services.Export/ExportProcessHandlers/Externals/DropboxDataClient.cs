using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class DropboxDataClient : IExternalDataClient
    {
        private readonly ILogger<DropboxDataClient> logger;

        private DropboxClient client;
        private TenantInfo tenant;
        
        public DropboxDataClient(ILogger<DropboxDataClient> logger)
        {
            this.logger = logger;
        }

        public void InitializeDataClient(string accessToken, string refreshToken, TenantInfo tenant)
        {
            this.client = new DropboxClient(accessToken);
            this.tenant = tenant;
            logger.LogTrace("Got Dropbox client");
        }

        private string Join(params string[] path) 
            => string.Join("/", path.Where(p => p != null));

        public Task<string> CreateApplicationFolderAsync(string subFolder) =>
            Task.FromResult("/" + Join(tenant.Name, subFolder).TrimStart('/'));

        public Task<string> CreateFolderAsync(string folder, string parentFolder)
            => Task.FromResult("/" + Join(parentFolder, folder).TrimStart('/'));

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            logger.LogTrace("Uploading file: {folder}/{fileName} - {length}bytes", folder, fileName, contentLength);

            await this.client.Files.UploadAsync(
                new CommitInfo(Join(folder, fileName), WriteMode.Overwrite.Instance), 
                fileStream);
            
            logger.LogTrace("Done Uploading file: {folder}/{fileName} - {length}bytes", folder, fileName, contentLength);
        }

        public async Task<long?> GetFreeSpaceAsync()
        {
            var storageInfo = await this.client.Users.GetSpaceUsageAsync();
            if (storageInfo == null) return null;

            var allocated = storageInfo.Allocation?.AsIndividual?.Value?.Allocated ??
                            storageInfo.Allocation?.AsTeam?.Value?.Allocated;

            if (allocated == null) return null;

            return (long)(allocated - storageInfo.Used);
        }

        public void Dispose()
        {
            this.client?.Dispose();
            this.client = null;
        }
    }
}
