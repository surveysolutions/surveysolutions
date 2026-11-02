using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class WorkspaceS3BinaryDataStorage : IWorkspaceBinaryDataStorage
    {
        private const int MaxDeletionBatches = 1000;
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
            int batchCount = 0;
            while (batchCount < MaxDeletionBatches)
            {
                var files = await externalFileStorage.ListAsync(string.Empty);
                if (files == null || !files.Any())
                    break;

                logger.LogWarning("Deleting {count} binary data files from S3 for workspace (batch {batch})",
                    files.Count, batchCount + 1);
                await externalFileStorage.RemoveAsync(files.Select(f => f.Path));
                batchCount++;
            }

            if (batchCount >= MaxDeletionBatches)
            {
                logger.LogError("Reached maximum batch count ({max}) while deleting binary data from S3 for workspace. Some files may not have been deleted.", MaxDeletionBatches);
            }
        }
    }
}
