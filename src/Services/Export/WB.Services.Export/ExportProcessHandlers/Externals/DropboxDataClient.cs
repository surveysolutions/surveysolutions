using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;

namespace WB.Services.Export.ExportProcessHandlers.Externals
{
    internal class DropboxDataClient : IExternalDataClient
    {
        private readonly ILogger<DropboxDataClient> logger;

        private DropboxClient client;

        public DropboxDataClient(ILogger<DropboxDataClient> logger)
        {
            this.logger = logger;
        }

        public IDisposable GetClient(string accessToken)
        {
            this.client = new DropboxClient(accessToken);
            logger.LogTrace("Got Dropbox client");
            return client;
        }

        public Task<string> CreateApplicationFolderAsync() => Task.FromResult(string.Empty);

        public Task<string> CreateFolderAsync(string applicationFolder, string folderName)
            => Task.FromResult($"/{folderName}");

        public async Task UploadFileAsync(string folder, string fileName, Stream fileStream, long contentLength, CancellationToken cancellationToken = default)
        {
            logger.LogTrace("Uploading file: {folder}/{fileName} - {length}bytes", folder, fileName, contentLength);

            await this.client.Files.UploadAsync(
                new CommitInfo($"{folder}/{fileName}", WriteMode.Overwrite.Instance), 
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
    }
}
