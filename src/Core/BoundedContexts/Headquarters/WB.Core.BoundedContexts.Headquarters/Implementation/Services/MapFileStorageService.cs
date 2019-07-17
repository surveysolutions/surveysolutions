using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class MapFileStorageService : IMapStorageService
    {
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;
        private readonly IPlainStorageAccessor<UserMap> userMapsStorage;
        private readonly IMapService mapPropertiesProvider;
        private readonly IUserRepository userStorage;

        
        private readonly IExternalFileStorage externalFileStorage;

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private const string TempFolderName = "TempMapsData";
        private const string MapsFolderName = "MapsData";
        private readonly string path;

        private readonly string[] permittedFileExtensions = { ".tpk", ".mmpk" };

        private readonly string mapsFolderPath;

        public MapFileStorageService(IFileSystemAccessor fileSystemAccessor, string folderPath, IArchiveUtils archiveUtils,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IPlainStorageAccessor<UserMap> userMapsStorage,
            IMapService mapPropertiesProvider,
            IUserRepository userStorage,
            IExternalFileStorage externalFileStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;

            this.mapPropertiesProvider = mapPropertiesProvider;

            this.userMapsStorage = userMapsStorage;
            this.userStorage = userStorage;

            this.externalFileStorage = externalFileStorage;

            this.path = fileSystemAccessor.CombinePath(folderPath, TempFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            this.mapsFolderPath = fileSystemAccessor.CombinePath(folderPath, MapsFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.mapsFolderPath))
                fileSystemAccessor.CreateDirectory(this.mapsFolderPath);
        }

        public string GetExternalStoragePath(string name) => $"maps/" + name;

        public async Task SaveOrUpdateMapAsync(ExtractedFile mapFile)
        {
            var tempFileStore = Guid.NewGuid().FormatGuid();
            var pathToSave = this.fileSystemAccessor.CombinePath(this.path, tempFileStore);
            if (!this.fileSystemAccessor.IsDirectoryExists(pathToSave))
                this.fileSystemAccessor.CreateDirectory(pathToSave);

            var tempFile = this.fileSystemAccessor.CombinePath(pathToSave, mapFile.Name);

            this.fileSystemAccessor.WriteAllBytes(tempFile, mapFile.Bytes);

            try
            {
                var properties = await mapPropertiesProvider.GetMapPropertiesFromFileAsync(tempFile);
                var mapItem = new MapBrowseItem()
                {
                    Id = mapFile.Name,
                    ImportDate = DateTime.UtcNow,
                    FileName = mapFile.Name,
                    Size = mapFile.Size,

                    Wkid = properties.Wkid,
                    XMaxVal = properties.XMax,
                    YMaxVal = properties.YMax,
                    XMinVal = properties.XMin,
                    YMinVal = properties.YMin,
                    MaxScale = properties.MaxScale,
                    MinScale = properties.MinScale
                };

                if (externalFileStorage.IsEnabled())
                {
                    using (FileStream file = File.OpenRead(tempFile))
                    {
                        var name = this.fileSystemAccessor.GetFileName(tempFile);
                        await this.externalFileStorage.StoreAsync(GetExternalStoragePath(name), file, "application/zip");
                    }
                }
                else
                {
                    var targetFile = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapFile.Name);
                    fileSystemAccessor.MoveFile(tempFile, targetFile);
                }
                
                this.mapPlainStorageAccessor.Store(mapItem, mapItem.Id);
            }
            catch
            {
                if (this.fileSystemAccessor.IsFileExists(tempFile))
                    fileSystemAccessor.DeleteFile(tempFile);
                throw;
            }
            finally
            {
                if (this.fileSystemAccessor.IsDirectoryExists(pathToSave))
                    fileSystemAccessor.DeleteDirectory(pathToSave);
            }
        }

        public void DeleteMap(string mapName)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map != null)
                this.mapPlainStorageAccessor.Remove(mapName);

            if (externalFileStorage.IsEnabled())
            {
                this.externalFileStorage.Remove(GetExternalStoragePath(mapName));
            }
            else
            {
                var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

                if (this.fileSystemAccessor.IsFileExists(filePath))
                    fileSystemAccessor.DeleteFile(filePath);
            }
        }

        public void DeleteMapUserLink(string map, string user)
        {
            if(string.IsNullOrWhiteSpace(map)||string.IsNullOrWhiteSpace(user))
                return;

            var mapUsers = this.userMapsStorage.Query(q=> q.Where(x=>x.Map==map && x.UserName == user)).ToList();
            if (mapUsers.Count > 0)
                this.userMapsStorage.Remove(mapUsers);
        }

        public ReportView GetAllMapUsersReportView()
        {
            var pageSize = 1024;
            var page = 1;

            List<string[]> rows = new List<string[]>();

            do
            {
                List<string> itemIds = this.mapPlainStorageAccessor.Query(queryable =>
                {
                    IQueryable<MapBrowseItem> pagedResults = queryable.OrderBy(x =>x.FileName).Skip((page - 1) * pageSize).Take(pageSize);

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

            
            return new ReportView()
            {
                Headers = new [] {"map", "users"},
                Data = rows.ToArray()
            };
        }

        public void UpdateUserMaps(string mapName, string[] users)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                throw new ArgumentException($"Map was not found {mapName}", nameof(mapName));

            var userMaps = userMapsStorage.Query(q => q.Where(x=>x.Map == mapName).ToList());

            var interviewerRoleId = UserRoles.Interviewer.ToUserId();

            var availableUsers = this.userStorage.Users
                .Where(x => users.Select(em => em.ToLower()).Contains(x.UserName.ToLower()))
                .Select(x => new
                {
                    UserName = x.UserName,
                    IsArchived = x.IsArchived,
                    IsInterviewer = x.Roles.Any(role => role.RoleId == interviewerRoleId)
                }).ToArray();
                
            var userMappings = availableUsers.Where(y => y.IsArchived == false && y.IsInterviewer == true)
                .Select(x => new UserMap() {Map = mapName, UserName = x.UserName}).ToList();

            userMapsStorage.Remove(userMaps);
            userMapsStorage.Store(userMappings.Select(x => Tuple.Create(x, (object)x)));
        }

        public string[] GetAllMapsForSupervisor(Guid supervisorId)
        {
            var interviewerNames = this.userStorage.Users
                .Where(x => supervisorId == x.Profile.SupervisorId && x.IsArchived == false)
                .Select(x => x.UserName)
                .ToArray();

            return userMapsStorage.Query(q =>
            {
                return q.Where(x => interviewerNames.Contains(x.UserName))
                        .Select(y => y.Map)
                        .Distinct();
            }).ToArray();
        }

        public string[] GetAllMapsForInterviewer(string userName)
        {
            return userMapsStorage.Query(q =>
            {
                return q.Where(x => x.UserName == userName)
                        .Select(y => y.Map)
                        .ToList();
            }).ToArray();
        }

        public byte[] GetMapContent(string mapName)
        {
            if (externalFileStorage.IsEnabled())
            {
                return this.externalFileStorage.GetBinary((GetExternalStoragePath(mapName)));
            }
            
            var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(filePath);
        }
    }
}
