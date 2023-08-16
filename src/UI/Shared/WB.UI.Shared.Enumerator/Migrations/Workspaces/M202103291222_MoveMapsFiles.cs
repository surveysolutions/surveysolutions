﻿using System.Threading.Tasks;
using NLog.Common;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Services;
using Xamarin.Essentials;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(202103291222)]
    public class M202103291222_MoveMapsFiles : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPrincipal principal;
        private readonly IPermissionsService permissions;
        private readonly ILogger logger;

        public M202103291222_MoveMapsFiles(IFileSystemAccessor fileSystemAccessor,
            IPrincipal principal,
            IPermissionsService permissions,
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.principal = principal;
            this.permissions = permissions;
            this.logger = logger;
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
            
            logger.Warn($"M202103291222_MoveMapsFiles: moving map files from {originPath} to {newPath}");

            bool hasPermissions = false;

            var task = Task.Run(async () =>
            {
                var status = await this.permissions.CheckPermissionStatusAsync<Permissions.StorageWrite>();
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
                logger.Warn($"M202103291222_MoveMapsFiles: moved file {fileName} from {file} to {newFilePath}");
            }
        }
    }
}