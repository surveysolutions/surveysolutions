using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public class WorkspaceFileSystemBinaryDataStorage : IWorkspaceBinaryDataStorage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IOptions<FileStorageConfig> fileStorageConfig;
        private readonly ILogger<WorkspaceFileSystemBinaryDataStorage> logger;

        public WorkspaceFileSystemBinaryDataStorage(
            IFileSystemAccessor fileSystemAccessor,
            IOptions<FileStorageConfig> fileStorageConfig,
            ILogger<WorkspaceFileSystemBinaryDataStorage> logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.fileStorageConfig = fileStorageConfig;
            this.logger = logger;
        }

        public Task DeleteAllBinaryDataForWorkspaceAsync()
        {
            var workspaceDataPath = fileStorageConfig.Value.AppData;

            if (fileSystemAccessor.IsDirectoryExists(workspaceDataPath))
            {
                logger.LogWarning("Deleting workspace binary data directory: {path}", workspaceDataPath);
                fileSystemAccessor.DeleteDirectory(workspaceDataPath);
            }

            return Task.CompletedTask;
        }
    }
}
