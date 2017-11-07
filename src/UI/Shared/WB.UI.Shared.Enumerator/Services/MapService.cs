using System.Collections.Generic;
using System.Linq;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.MapService;

namespace WB.UI.Shared.Enumerator.Services
{
    public class MapService : IMapService
    {
        private readonly IPermissions permissions;
        private readonly IFileSystemAccessor fileSystemAccessor;
       
        private readonly string mapsLocation;
        private readonly ILogger logger;

        string filesToSearch = "*.tpk";
        
        public MapService(IPermissions permissions, 
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger)
        {
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            
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
                    Size = this.fileSystemAccessor.GetFileSize(x),
                    MapName = this.fileSystemAccessor.GetFileNameWithoutExtension(x),
                    CreationDate = this.fileSystemAccessor.GetCreationTime(x)

                }).ToList();
        }

        public bool DoesMapExist(string mapName)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                return false;

            var filename = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapName);

            return this.fileSystemAccessor.IsFileExists(filename);
        }

        public void SaveMap(string mapName, byte[] content)
        {
            if (!DoesMapExist(mapName))
            {
                var filename = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapName);

                this.fileSystemAccessor.WriteAllBytes(filename, content);
            }
        }
    }
}