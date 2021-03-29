using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(202103291222)]
    public class M202103291222_CreateMapsTable : IMigration
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainStorage<New.MapDescriptionView> mapsStorage;
        private readonly IPrincipal principal;
        
        public class New
        {
            [Workspaces]
            public class MapDescriptionView: IPlainStorageEntity
            {
                [PrimaryKey]
                public string Id { get; set; }
                public string MapName { get; set; }
                public string Workspace { get; set; }
            }
        }

        public M202103291222_CreateMapsTable(IFileSystemAccessor fileSystemAccessor,
            IPlainStorage<New.MapDescriptionView> mapsStorage,
            IPrincipal principal)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.mapsStorage = mapsStorage;
            this.principal = principal;
        }

        public void Up()
        {
            var workspace = principal.CurrentUserIdentity?.Workspace ?? "primary";
            if (string.IsNullOrWhiteSpace(workspace))
                return;
            
            var mapsLocation = fileSystemAccessor.CombinePath(AndroidPathUtils.GetPathToExternalDirectory(), "TheWorldBank/Shared/MapCache/");

            var mapDescriptions = GetAvailableMapNames(mapsLocation);
            if (mapDescriptions.Count == 0)
                return;

            var mapDescriptionViews = mapDescriptions.Select(mapName => new New.MapDescriptionView()
            {
                Workspace = workspace,
                Id = $"{mapName}_{workspace}",
                MapName = mapName,
            }).ToList();
            
            if (mapDescriptionViews.Any())
                mapsStorage.Store(mapDescriptionViews);
        }
        
        private List<string> GetAvailableMapNames(string mapsLocation)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(mapsLocation))
                return new List<string>();

            string[] mapFilesToSearch = { "*.tpk", "*.tpkx", "*.mmpk", "*.mmpkx",  "*.tif" };
            
            var localMaps = mapFilesToSearch
                .SelectMany(i => this.fileSystemAccessor.GetFilesInDirectory(mapsLocation, i))
                .OrderBy(x => x)
                .Select(x => this.fileSystemAccessor.GetFileNameWithoutExtension(x))
                .ToList();

            return localMaps;
        }
    }
}