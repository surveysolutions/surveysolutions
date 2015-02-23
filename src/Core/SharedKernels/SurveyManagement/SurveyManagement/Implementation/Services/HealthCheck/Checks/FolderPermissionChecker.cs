using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
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

            var directories = GetAllSubFolders(folderPath, deniedFolders);

            var windowsIdentity = WindowsIdentity.GetCurrent();

            foreach (var directory in directories)
            {
                var writeAccess = CheckWriteAccess(windowsIdentity, directory);

                if (writeAccess)
                {
                    allowedFolders.Add(directory);
                }
                else
                {
                    deniedFolders.Add(directory);
                }
            }

            var status = deniedFolders.Any() ? HealthCheckStatus.Down : HealthCheckStatus.Happy; 


            FolderPermissionCheckResult result = new FolderPermissionCheckResult(
                status: status,
                processRunedUnder: windowsIdentity.Name,
                allowedFolders: allowedFolders.ToArray(),
                denidedFolders: deniedFolders.ToArray());

            return result;
        }

        private IEnumerable<string> GetAllSubFolders(string path, HashSet<string> deniedFolders)
        {
            HashSet<string> folders = new HashSet<string>();
            folders.Add(path);

            try
            {
                var directories = fileSystemAccessor.GetDirectoriesInDirectory(path);
                folders.UnionWith(directories);
            }
            catch (UnauthorizedAccessException)
            {
                deniedFolders.Add(path);
                return Enumerable.Empty<string>();
            }

            return folders;
        }

        public bool CheckWriteAccess(WindowsIdentity currentUserReference, string path)
        {
            if (string.IsNullOrEmpty(path) || !fileSystemAccessor.IsDirectoryExists(path)) 
                return false;

            return fileSystemAccessor.IsWritePermissionExists(path);
        }
    }
}