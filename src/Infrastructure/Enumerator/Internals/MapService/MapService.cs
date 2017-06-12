using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Infrastructure.Shared.Enumerator.Internals.MapService
{
    public class MapService : IMapService
    {
        private readonly IPermissions permissions;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IMapSynchronizer mapSynchronizer;
        private readonly string mapsLocation;

        string filesToSearch = "*.tpk";
        
        public MapService(IPermissions permissions, 
            IFileSystemAccessor fileSystemAccessor,
            IMapSynchronizer mapSynchronizer)
        {
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapSynchronizer = mapSynchronizer;
            var pathToRootDirectory = Build.VERSION.SdkInt < BuildVersionCodes.N ? AndroidPathUtils.GetPathToExternalDirectory() : AndroidPathUtils.GetPathToInternalDirectory();
            
            this.mapsLocation = fileSystemAccessor.CombinePath(pathToRootDirectory, "TheWorldBank/Shared/MapCache/");
        }

        public Dictionary<string, string> GetAvailableMaps()
        {

            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                return new Dictionary<string, string>();

            var tpkFileSearchResult = this.fileSystemAccessor.GetFilesInDirectory(this.mapsLocation, this.filesToSearch).OrderBy(x => x).ToList();
            if (tpkFileSearchResult.Count == 0)
                return new Dictionary<string, string>();

            return tpkFileSearchResult.ToDictionary(this.fileSystemAccessor.GetFileNameWithoutExtension);
        }

        public async Task SyncMaps(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                this.fileSystemAccessor.CreateDirectory(this.mapsLocation);
            try
            {
                await this.mapSynchronizer.SyncMaps(this.mapsLocation, cancellationToken);
            }
            catch
            {
            }
            
        }
    }
}