using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Shared.Enumerator.Services.Internals.MapService
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
            
            this.mapsLocation = fileSystemAccessor.CombinePath(AndroidPathUtils.GetPathToExternalDirectory(), "TheWorldBank/Shared/MapCache/");
        }

        public List<MapDescription> GetAvailableMaps()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                return new List<MapDescription>();

            return this.fileSystemAccessor.GetFilesInDirectory(this.mapsLocation, this.filesToSearch).OrderBy(x => x)
                .Select(x => new MapDescription()
                {
                    MapFullPath = x,
                    MapName = this.fileSystemAccessor.GetFileNameWithoutExtension(x)
                }).ToList();
        }

        public async Task SyncMaps(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                this.fileSystemAccessor.CreateDirectory(this.mapsLocation);
            
            var items = await this.mapSynchronizer.GetMapList(cancellationToken).ConfigureAwait(false);

            foreach (var mapDescription in items)
            {
                var filename = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapDescription.MapName);

                if (this.fileSystemAccessor.IsFileExists(filename))
                    continue;

                var mapContent = await this.mapSynchronizer.GetMapContent(mapDescription.URL, cancellationToken).ConfigureAwait(false);

                this.fileSystemAccessor.WriteAllBytes(filename, mapContent);
            }

        }
    }
}