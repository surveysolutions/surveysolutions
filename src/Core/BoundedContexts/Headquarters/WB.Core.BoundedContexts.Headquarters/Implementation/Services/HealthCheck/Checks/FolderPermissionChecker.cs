using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    public class FolderPermissionChecker : IAtomicHealthCheck<FolderPermissionCheckResult>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string folderPath;

        public FolderPermissionChecker(string folderPath, IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.folderPath = folderPath;
        }

        public FolderPermissionCheckResult Check()
        {
            HashSet<string> allowedFolders = new HashSet<string>();
            HashSet<string> deniedFolders = new HashSet<string>();

            var directories = this.GetAllSubFolders(this.folderPath, deniedFolders);

            var windowsIdentity = WindowsIdentity.GetCurrent();

            foreach (var directory in directories)
            {
                var writeAccess = this.CheckWriteAccess(directory);
                var directoryName = this.fileSystemAccessor.GetFileName(directory);
                if (writeAccess)
                {
                    allowedFolders.Add(directoryName);
                }
                else
                {
                    deniedFolders.Add(directoryName);
                }
            }

            var status = deniedFolders.Any() ? HealthCheckStatus.Down : HealthCheckStatus.Happy; 


            FolderPermissionCheckResult result = new FolderPermissionCheckResult(
                status: status,
                processRunedUnder: windowsIdentity.Name,
                allowedFolders: allowedFolders.ToArray(),
                deniedFolders: deniedFolders.ToArray());

            return result;
        }

        private IEnumerable<string> GetAllSubFolders(string path, HashSet<string> deniedFolders)
        {
            HashSet<string> folders = new HashSet<string>();
            folders.Add(path);

            try
            {
                var directories = this.fileSystemAccessor.GetDirectoriesInDirectory(path);
                folders.UnionWith(directories);
            }
            catch (UnauthorizedAccessException)
            {
                deniedFolders.Add(path);
                return Enumerable.Empty<string>();
            }

            return folders;
        }

        public bool CheckWriteAccess(string path)
        {
            if (string.IsNullOrEmpty(path) || !this.fileSystemAccessor.IsDirectoryExists(path)) 
                return false;

            return this.fileSystemAccessor.IsWritePermissionExists(path);
        }
    }
}