using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
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
        private readonly string shapefilesLocation;
        private readonly ILogger logger;

        string[] mapFilesToSearch = { "*.tpk", "*.mmpk", "*.tif" };

        string[] shapefilesToSearch = { "*.shp"};

        string tempSuffix = ".part";

        public MapService(IPermissions permissions,
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger)
        {
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;

            this.mapsLocation = fileSystemAccessor.CombinePath(AndroidPathUtils.GetPathToExternalDirectory(), "TheWorldBank/Shared/MapCache/");
            this.shapefilesLocation = fileSystemAccessor.CombinePath(AndroidPathUtils.GetPathToExternalDirectory(), "TheWorldBank/Shared/ShapefileCache");
        }


        public MapDescription PrepareAndGetDefaultMap()
        {
            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                : AndroidPathUtils.GetPathToExternalDirectory();

            string mapFolderPath = this.fileSystemAccessor.CombinePath(basePath, "maps");
            string mapPath = this.fileSystemAccessor.CombinePath(mapFolderPath, "worldmap(default).tpk");

            if (!this.fileSystemAccessor.IsFileExists(mapPath))
            {
                if (!this.fileSystemAccessor.IsDirectoryExists(mapFolderPath))
                    this.fileSystemAccessor.CreateDirectory(mapFolderPath);

                using (var br = new BinaryReader(Application.Context.Assets.Open("worldmap(default).tpk")))
                {
                    using (var bw = new BinaryWriter(new FileStream(mapPath, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }
            }

            var defaultMap = new MapDescription() { MapName = "Worldmap[default]", MapFullPath = mapPath };
            return defaultMap;
        }

        public List<MapDescription> GetAvailableMaps()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                return new List<MapDescription>();

            return
                this.mapFilesToSearch
                .SelectMany(i => this.fileSystemAccessor.GetFilesInDirectory(this.mapsLocation, i))
                .OrderBy(x => x)
                .Select(x => new MapDescription()
                {
                    MapFullPath = x,
                    Size = this.fileSystemAccessor.GetFileSize(x),
                    MapName = this.fileSystemAccessor.GetFileNameWithoutExtension(x),
                    MapFileName = this.fileSystemAccessor.GetFileName(x),
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

        public Stream GetTempMapSaveStream(string mapName)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                this.fileSystemAccessor.CreateDirectory(this.mapsLocation);

            var tempFileName = GetTempFileName(mapName);

            if (this.fileSystemAccessor.IsFileExists(tempFileName))
                this.fileSystemAccessor.DeleteFile(tempFileName);

            return this.fileSystemAccessor.OpenOrCreateFile(tempFileName, false);

        }

        public void MoveTempMapToPermanent(string mapName)
        {
            var tempFileName = GetTempFileName(mapName);

            if (!this.fileSystemAccessor.IsFileExists(tempFileName))
                return;

            var newName = this.fileSystemAccessor.ChangeExtension(tempFileName, null);

            this.fileSystemAccessor.MoveFile(tempFileName, newName);
        }

        public void RemoveMap(string mapName)
        {
            var filename = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapName);

            if (this.fileSystemAccessor.IsFileExists(filename))
                this.fileSystemAccessor.DeleteFile(filename);
        }

        public List<ShapefileDescription> GetAvailableShapefiles()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.shapefilesLocation))
                return new List<ShapefileDescription>();

            return
                this.shapefilesToSearch
                    .SelectMany(i => this.fileSystemAccessor.GetFilesInDirectory(this.shapefilesLocation, i))
                    .OrderBy(x => x)
                    .Select(x => new ShapefileDescription()
                    {
                        FullPath = x,
                        ShapefileName = this.fileSystemAccessor.GetFileNameWithoutExtension(x)

                    }).ToList();
        }

        private string GetTempFileName(string mapName)
        {
            var fileName = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapName);
            var tempFileName = fileName + tempSuffix;
            return tempFileName;
        }
    }
}
