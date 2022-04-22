#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;

namespace WB.UI.Shared.Enumerator.Services
{
    public class MapService : IMapService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly string mapsLocationCommon;
        private readonly string shapefilesLocationCommon;
        
        private readonly ILogger logger;
        private readonly IEnumeratorArchiveUtils archiveUtils;

        private readonly string[] mapFilesToSearch = { "*.tpk", "*.tpkx", "*.mmpk", "*.mmpkx",  "*.tif" };
        private readonly string[] shapefilesToSearch = { "*.shp"};
        private string tempSuffix = ".part";

        private readonly string? workspaceName = null;
        
        public MapService(
            IFileSystemAccessor fileSystemAccessor,
            ILogger logger,
            IWorkspaceAccessor workspaceAccessor,
            IEnumeratorArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.archiveUtils = archiveUtils;
            this.workspaceName = workspaceAccessor.GetCurrentWorkspaceName();
            
            this.mapsLocationCommon = fileSystemAccessor.CombinePath(
                AndroidPathUtils.GetPathToExternalDirectory(), 
                "TheWorldBank/Shared/MapCache/");
            
            this.shapefilesLocationCommon = fileSystemAccessor.CombinePath(
                AndroidPathUtils.GetPathToExternalDirectory(), 
                "TheWorldBank/Shared/ShapefileCache");
        }

        private string GetMapsLocationOrThrow()
        {
            CheckWorkspaceAndThrow();
            return fileSystemAccessor.CombinePath(this.mapsLocationCommon, workspaceName);
        }

        private void CheckWorkspaceAndThrow()
        {
            if (string.IsNullOrEmpty(workspaceName))
            {
                logger.Error("Workspace name is empty;");
                throw new InvalidOperationException("Workspace name is empty;");
            }
        }

        public MapDescription? PrepareAndGetDefaultMapOrNull()
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

                if (Application.Context.Assets != null)
                {
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
            }

            if (!this.fileSystemAccessor.IsFileExists(mapPath)) return null;
            
            var defaultMap = new MapDescription(MapType.LocalFile, "Worldmap[default]")
            {
                MapFullPath = mapPath
            };
            return defaultMap;
        }

        public List<MapDescription> GetAvailableMaps(bool includeOnline = false)
        {
            var mapList = new List<MapDescription>();

            if (includeOnline)
            {
                mapList.Add(new MapDescription(MapType.OnlineImagery, "Online: Imagery"));
                mapList.Add(new MapDescription(MapType.OnlineImageryWithLabels, "Online: Imagery with labels"));
                mapList.Add(new MapDescription(MapType.OnlineOpenStreetMap, "Online: Open Street Map"));
            }

            void AddMapsFromFolder(string folder)
            {
                if (!this.fileSystemAccessor.IsDirectoryExists(folder))
                    return;

                var localMaps = this.mapFilesToSearch
                    .SelectMany(i => this.fileSystemAccessor.GetFilesInDirectory(folder, i, true))
                    .OrderBy(x => x)
                    .Select(x =>
                        new MapDescription(MapType.LocalFile, this.fileSystemAccessor.GetFileNameWithoutExtension(x))
                        {
                            MapFullPath = x,
                            Size = this.fileSystemAccessor.GetFileSize(x),
                            MapFileName = this.fileSystemAccessor.GetFileName(x),
                            CreationDate = this.fileSystemAccessor.GetCreationTime(x)
                        }).ToList();

                var newMaps = localMaps.Where(m => mapList.All(ml => ml.MapName != m.MapName)).ToList();
                if (newMaps.Count > 0)
                    mapList.AddRange(newMaps);
            }
            
            if(!string.IsNullOrEmpty(workspaceName))
                AddMapsFromFolder(GetMapsLocationOrThrow());

            AddMapsFromFolder(this.mapsLocationCommon);

            return mapList;
        }

        public bool DoesMapExist(string mapName)
        {
            if (!string.IsNullOrEmpty(workspaceName))
            {
                var filename = this.fileSystemAccessor.CombinePath(GetMapsLocationOrThrow(), mapName);
                if (this.fileSystemAccessor.IsFileExists(filename))
                    return true;
                if (this.fileSystemAccessor.IsDirectoryExists(filename))
                    return true;
            }

            var filenameInCommonFolder = this.fileSystemAccessor.CombinePath(this.mapsLocationCommon, mapName);
            return this.fileSystemAccessor.IsFileExists(filenameInCommonFolder);
        }

        public Stream GetTempMapSaveStream(string mapName)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(GetMapsLocationOrThrow()))
                this.fileSystemAccessor.CreateDirectory(GetMapsLocationOrThrow());

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

            var fileExtension = fileSystemAccessor.GetFileExtension(mapName);
            if (fileExtension == ".shp") // shape file package
            {
                var mapFolder = this.fileSystemAccessor.CombinePath(GetMapsLocationOrThrow(), mapName);
                try
                {
                    if (fileSystemAccessor.IsDirectoryExists(mapFolder))
                        fileSystemAccessor.DeleteDirectory(mapFolder);
                    fileSystemAccessor.CreateDirectory(mapFolder);
                    archiveUtils.Unzip(tempFileName, mapFolder);
                }
                catch
                {
                    if (fileSystemAccessor.IsDirectoryExists(mapFolder))
                        fileSystemAccessor.DeleteDirectory(mapFolder);
                    throw;
                }
                fileSystemAccessor.DeleteFile(tempFileName);
            }
            else
            {
                var newName = this.fileSystemAccessor.ChangeExtension(tempFileName, null);
                this.fileSystemAccessor.MoveFile(tempFileName, newName);
            }            
        }

        public void RemoveMap(string mapName)
        {
            var filename = this.fileSystemAccessor.CombinePath(GetMapsLocationOrThrow(), mapName);
            if (this.fileSystemAccessor.IsFileExists(filename))
                this.fileSystemAccessor.DeleteFile(filename);

            if (this.fileSystemAccessor.IsDirectoryExists(filename))
                this.fileSystemAccessor.DeleteDirectory(filename);

            var filenameInCommon = this.fileSystemAccessor.CombinePath(this.mapsLocationCommon, mapName);
            if (this.fileSystemAccessor.IsFileExists(filenameInCommon))
                this.fileSystemAccessor.DeleteFile(filenameInCommon);
        }

        public List<ShapefileDescription> GetAvailableShapefiles()
        {
            List<ShapefileDescription> GetShapesInFolder(string path)
            {
                if (!this.fileSystemAccessor.IsDirectoryExists(path))
                    return new List<ShapefileDescription>();

                return this.shapefilesToSearch
                    .SelectMany(i => this.fileSystemAccessor.GetFilesInDirectory(path, i, true))
                    .OrderBy(x => x)
                    .Select(x => new ShapefileDescription()
                    {
                        FullPath = x,
                        ShapefileName = this.fileSystemAccessor.GetFileNameWithoutExtension(x),
                        ShapefileFileName = this.fileSystemAccessor.GetFileName(x),
                        CreationDate = this.fileSystemAccessor.GetCreationTime(x),
                        Size = this.fileSystemAccessor.GetFileSize(x),
                    }).ToList();
            }

            List<ShapefileDescription> shapefileDescriptions = new List<ShapefileDescription>();

            if (!string.IsNullOrEmpty(workspaceName))
            {
                var shapesInFolder = GetShapesInFolder(fileSystemAccessor.CombinePath(
                    this.shapefilesLocationCommon, workspaceName));
                shapefileDescriptions.AddRange(shapesInFolder);

                var shapesInMapFolder = GetShapesInFolder(fileSystemAccessor.CombinePath(
                    this.mapsLocationCommon, workspaceName));
                shapefileDescriptions.AddRange(shapesInMapFolder);
            }

            var shapesInCommonFolder = GetShapesInFolder(this.shapefilesLocationCommon);
            shapefileDescriptions.AddRange(shapesInCommonFolder);

            return shapefileDescriptions;
        }

        private string GetTempFileName(string mapName)
        {
            var fileName = this.fileSystemAccessor.CombinePath(GetMapsLocationOrThrow(), mapName);
            var tempFileName = fileName + tempSuffix;
            return tempFileName;
        }
    }
}
