using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class WorkspaceS3BinaryDataStorage : IWorkspaceBinaryDataStorage
    {
        private readonly IExternalFileStorage externalFileStorage;
        private readonly ILogger<WorkspaceS3BinaryDataStorage> logger;

        public WorkspaceS3BinaryDataStorage(
            IExternalFileStorage externalFileStorage,
            ILogger<WorkspaceS3BinaryDataStorage> logger)
        {
            this.externalFileStorage = externalFileStorage;
            this.logger = logger;
        }

        public async Task DeleteAllBinaryDataForWorkspaceAsync()
        {
            while (true)
            {
                var files = await externalFileStorage.ListAsync(string.Empty);
                if (files == null || !files.Any())
                    break;

                logger.LogWarning("Deleting {count} binary data files from S3 for workspace", files.Count);
                await externalFileStorage.RemoveAsync(files.Select(f => f.Path));
            }
        }
    }
}
