using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class FileSystemMapRepository : IMapRepository
    {
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;
        private readonly IPlainStorageAccessor<UserMap> userMapsStorage;
        private readonly IMapPropertiesProvider mapPropertiesProvider;
        private readonly IUserRepository userStorage;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private const string TempFolderName = "TempMapsData";
        private const string UnzippedFoldername = "Unzipped";
        private const string MapsFolderName = "MapsData";
        private readonly string path;

        private readonly string[] permittedFileExtensions = { ".tpk" };

        private readonly string mapsFolderPath;

        private static ILogger Logger => ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<FileSystemMapRepository>();

        private static readonly HashSet<string> createdFolders = new HashSet<string>();

        public FileSystemMapRepository(IFileSystemAccessor fileSystemAccessor, string folderPath, IArchiveUtils archiveUtils,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IPlainStorageAccessor<UserMap> userMapsStorage,
            IMapPropertiesProvider mapPropertiesProvider,
            IUserRepository userStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;

            this.mapPropertiesProvider = mapPropertiesProvider;

            this.userMapsStorage = userMapsStorage;
            this.userStorage = userStorage;

            this.path = fileSystemAccessor.CombinePath(folderPath, TempFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            this.mapsFolderPath = fileSystemAccessor.CombinePath(folderPath, MapsFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.mapsFolderPath))
                fileSystemAccessor.CreateDirectory(this.mapsFolderPath);
        }

        public string StoreData(Stream dataFile, string fileName)
        {
            var folderName = Guid.NewGuid().FormatGuid();
            var folderPath = this.fileSystemAccessor.CombinePath(this.path, folderName);

            if (this.fileSystemAccessor.IsDirectoryExists(folderPath))
                this.fileSystemAccessor.DeleteDirectory(folderPath);

            this.fileSystemAccessor.CreateDirectory(folderPath);

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(this.fileSystemAccessor.CombinePath(folderPath, this.fileSystemAccessor.GetFileName(fileName)), false))
            {
                dataFile.CopyTo(fileStream);
            }

            createdFolders.Add(folderName);

            return folderName;
        }


        public MapFileDescription[] GetMapsMetaInformation(string id)
        {
            var filesInDirectory = this.GetFiles(id);
            var zipFilesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
            if (zipFilesInDirectory.Length > 0)
            {
                try
                {
                    return this.archiveUtils.GetArchivedFileNamesAndSize(zipFilesInDirectory[0])
                                .Select(file => new MapFileDescription(file.Key, file.Value))
                                .ToArray();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            return null;
        }

        private IEnumerable<string> GetFiles(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
            if (!this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new string[0];

            return this.fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
        }

        public string[] UnzipAndGetFileList(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);
            if (!this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
                return new string[0];

            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(currentFolderPath);
            return this.TryToGetMapsFromZipArchive(filesInDirectory, id, currentFolderPath);
        }

        private string[] TryToGetMapsFromZipArchive(IEnumerable<string> filesInDirectory, string id, string currentFolderPath)
        {
            var archivesInDirectory = filesInDirectory.Where(this.archiveUtils.IsZipFile).ToArray();
            if (archivesInDirectory.Length == 0)
                return new string[0];

            var archivePath = archivesInDirectory[0];
            var unzippedDirectoryPath = this.fileSystemAccessor.CombinePath(currentFolderPath, UnzippedFoldername);

            if (!this.fileSystemAccessor.IsDirectoryExists(unzippedDirectoryPath))
            {
                try
                {
                    this.archiveUtils.Unzip(archivePath, unzippedDirectoryPath);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                    return null;
                }
            }
            var unzippedFiles = this.fileSystemAccessor.GetFilesInDirectory(unzippedDirectoryPath)
                .Where(file => this.permittedFileExtensions.Contains(this.fileSystemAccessor.GetFileExtension(file)))
                .ToArray();

            return unzippedFiles;
        }

        public async Task SaveOrUpdateMapAsync(string map)
        {

            this.fileSystemAccessor.CopyFileOrDirectory(map, this.mapsFolderPath, true);

            var filename = this.fileSystemAccessor.GetFileName(map);
            var properties = await mapPropertiesProvider.GetMapPropertiesFromFileAsync(map);

            var mapItem = new MapBrowseItem()
            {
                Id = filename,
                ImportDate = DateTime.UtcNow,
                FileName = filename,
                Size = this.fileSystemAccessor.GetFileSize(map),

                Wkid = properties.Wkid,
                XMaxVal = properties.XMax,
                YMaxVal = properties.YMax,
                XMinVal = properties.XMin,
                YMinVal = properties.YMin,
                MaxScale = properties.MaxScale,
                MinScale = properties.MinScale
            };

            this.mapPlainStorageAccessor.Store(mapItem, mapItem.Id);
        }

        public void DeleteData(string id)
        {
            var currentFolderPath = this.fileSystemAccessor.CombinePath(this.path, id);

            if (this.fileSystemAccessor.IsDirectoryExists(currentFolderPath))
            {
                try
                {
                    this.fileSystemAccessor.DeleteDirectory(currentFolderPath);
                }
                catch (Exception exception)
                {
                    Logger.Warn($"Failed to delete folder '{currentFolderPath}'. Will try next time.", exception);
                }
            }
        }

        public void DeleteMap(string mapName)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map != null)
                this.mapPlainStorageAccessor.Remove(mapName);

            var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

            if(this.fileSystemAccessor.IsFileExists(filePath))
                fileSystemAccessor.DeleteFile(filePath);
        }

        public void DeleteMapUser(string map, string user)
        {
            if(string.IsNullOrWhiteSpace(map)||string.IsNullOrWhiteSpace(user))
                return;

            var mapUsers = this.userMapsStorage.Query(q=> q.Where(x=>x.Map==map && x.UserName == user)).ToList();
            if (mapUsers.Count > 0)
                this.userMapsStorage.Remove(mapUsers);
        }

        public string[][] GetAllMapUsers()
        {
            var pageSize = 1024;
            var page = 1;

            List<string[]> rows = new List<string[]>();

            do
            {
                List<string> itemIds = this.mapPlainStorageAccessor.Query(queryable =>
                {
                    IQueryable<MapBrowseItem> pagedResults = queryable.Skip((page - 1) * pageSize).Take(pageSize);

                    itemIds = pagedResults.Select(x => x.Id).ToList();

                    return itemIds;
                });

                if(itemIds.Count<1)
                    break;

                page++;

                List<Tuple<string, string>> maps = userMapsStorage.Query(q =>
                {
                    return q.Select(x => new Tuple<string, string>(x.Map, x.UserName)).Where(x => itemIds.Contains(x.Item1))
                        .ToList();
                });

                rows.AddRange(
                    itemIds.Select(x => new[]
                        {
                            x,
                            string.Join(",", maps.Where(y => y.Item1 == x).Select(y => y.Item2).ToArray())
                        }
                    ));

            } while (true);



            return rows.ToArray();
        }

        public bool UpdateUserMaps(string mapName, string[] users)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                return false;

            var userMaps = userMapsStorage.Query(q => q.Where(x=>x.Map == mapName).ToList());
            
            var availableUsers = this.userStorage.Users.Where(x => users.Contains(x.UserName)).ToList();
            var userMappings = availableUsers.Select(x => new UserMap() {Map = mapName, UserName = x.UserName}).ToList();

            userMapsStorage.Remove(userMaps);
            userMapsStorage.Store(userMappings.Select(x => Tuple.Create(x, (object)x)));

            if(availableUsers.Count < users.Length)
                return false;

            return true;
        }

        public string[] GetAllMapsForUser(string userName)
        {
            return userMapsStorage.Query(q =>
            {
                return q.Where(x => x.UserName == userName).Select(y => y.Map).Distinct().ToList();
            }).ToArray();
        }

        public byte[] GetMapContent(string mapName)
        {
            var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(filePath);
        }
    }
}
