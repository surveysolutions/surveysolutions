using System.Threading.Tasks;
using NLog.Common;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(202103291222)]
    public class M202103291222_CreateMapsTable : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPrincipal principal;
        private readonly IPermissions permissions;

        public M202103291222_CreateMapsTable(IFileSystemAccessor fileSystemAccessor,
            IPrincipal principal,
            IPermissions permissions)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.principal = principal;
            this.permissions = permissions;
        }

        public void Up()
        {
            var workspace = principal.CurrentUserIdentity?.Workspace ?? "primary";
            if (string.IsNullOrWhiteSpace(workspace))
                return;

            var originMapsLocation = fileSystemAccessor.CombinePath(
                AndroidPathUtils.GetPathToExternalDirectory(), 
                "TheWorldBank/Shared/MapCache/");
            var newMapsLocation = fileSystemAccessor.CombinePath(
                originMapsLocation,
                workspace);
            MoveAllFiles(originMapsLocation, newMapsLocation);

            var originShapefilesLocation = fileSystemAccessor.CombinePath(
                AndroidPathUtils.GetPathToExternalDirectory(), 
                "TheWorldBank/Shared/ShapefileCache");
            var newShapefilesLocation = fileSystemAccessor.CombinePath(
                originShapefilesLocation,
                workspace);
            MoveAllFiles(originShapefilesLocation, newShapefilesLocation);
        }
        
        private void MoveAllFiles(string originPath, string newPath)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(originPath))
                return;

            bool hasPermissions = false;

            var task = Task.Run(async () =>
            {
                var status = await this.permissions.CheckPermissionStatusAsync<StoragePermission>();
                hasPermissions = status == PermissionStatus.Granted;
            });
            task.Wait();
            
            if (!hasPermissions)
                return;

            var files = this.fileSystemAccessor.GetFilesInDirectory(originPath);
            if (files.Length == 0)
                return;

            if (!this.fileSystemAccessor.IsDirectoryExists(newPath))
                fileSystemAccessor.CreateDirectory(newPath);

            foreach (var file in files)
            {
                var fileName = fileSystemAccessor.GetFileName(file);
                var newFilePath = fileSystemAccessor.CombinePath(newPath, fileName); 
                fileSystemAccessor.MoveFile(file, newFilePath);
            }
        }
    }
}