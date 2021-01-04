using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class MapFileStorageService : IMapStorageService
    {
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;
        private readonly IPlainStorageAccessor<UserMap> userMapsStorage;
        private readonly ISerializer serializer;
        private readonly IUserRepository userStorage;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IOptions<GeospatialConfig> geospatialConfig;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<MapFileStorageService> logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;

        private const int WGS84Wkid = 4326; //https://epsg.io/4326
        private const string TempFolderName = "TempMapsData";
        private const string MapsFolderName = "MapsData";
        private readonly string path;

        private readonly string mapsFolderPath;

        public MapFileStorageService(
            IFileSystemAccessor fileSystemAccessor,
            IOptions<FileStorageConfig> fileStorageConfig,
            IArchiveUtils archiveUtils,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IPlainStorageAccessor<UserMap> userMapsStorage,
            ISerializer serializer,
            IUserRepository userStorage,
            IExternalFileStorage externalFileStorage,
            IOptions<GeospatialConfig> geospatialConfig,
            IAuthorizedUser authorizedUser,
            ILogger<MapFileStorageService> logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.userMapsStorage = userMapsStorage;
            this.serializer = serializer;
            this.userStorage = userStorage;

            this.externalFileStorage = externalFileStorage;
            this.authorizedUser = authorizedUser;
            this.logger = logger;
            this.geospatialConfig = geospatialConfig;

            this.path = fileSystemAccessor.CombinePath(fileStorageConfig.Value.TempData, TempFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            this.mapsFolderPath = fileSystemAccessor.CombinePath(fileStorageConfig.Value.TempData, MapsFolderName);
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

            await this.fileSystemAccessor.WriteAllBytesAsync(tempFile, mapFile.Bytes)
                .ConfigureAwait(false);

            try
            {
                var mapItem = this.ToMapBrowseItem(tempFile, mapFile);

                if (externalFileStorage.IsEnabled())
                {
                    await using FileStream file = File.OpenRead(tempFile);
                    var name = this.fileSystemAccessor.GetFileName(tempFile);
                    await this.externalFileStorage.StoreAsync(GetExternalStoragePath(name), file, "application/zip")
                        .ConfigureAwait(false);
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

        private MapBrowseItem ToMapBrowseItem(string tempFile, ExtractedFile mapFile)
        {
            var item = new MapBrowseItem
            {
                Id = mapFile.Name,
                ImportDate = DateTime.UtcNow,
                FileName = mapFile.Name,
                Size = mapFile.Size,
                Wkid = 102100
            };

            void SetMapProperties(dynamic _, MapBrowseItem i)
            {
                i.Wkid = _.fullExtent.spatialReference.wkid;
                i.XMaxVal = _.fullExtent.xmax;
                i.XMinVal = _.fullExtent.xmin;
                i.YMaxVal = _.fullExtent.ymax;
                i.YMinVal = _.fullExtent.ymin;
                i.MaxScale = _.maxScale;
                i.MinScale = _.minScal;
            };

            switch (this.fileSystemAccessor.GetFileExtension(tempFile))
            {
                case ".tpk":
                    {
                        var unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, "conf.cdi");
                        if (unzippedFile != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(Encoding.UTF8.GetString(unzippedFile.Bytes));
                            var envelope = doc["EnvelopeN"];
                            item.Wkid = int.Parse(envelope["SpatialReference"]["WKID"].InnerText);
                            item.XMaxVal = double.Parse(envelope["XMax"].InnerText);
                            item.XMinVal = double.Parse(envelope["XMin"].InnerText);
                            item.YMaxVal = double.Parse(envelope["YMax"].InnerText);
                            item.YMinVal = double.Parse(envelope["YMin"].InnerText);
                        }
                        else
                        {
                            unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, "mapserver.json");
                            if (unzippedFile != null)
                            {
                                var jsonObject = this.serializer.Deserialize<dynamic>(Encoding.UTF8.GetString(unzippedFile.Bytes));
                                SetMapProperties(jsonObject.contents, item);
                            }
                        }
                    }
                    break;
                case ".mmpk":
                    {
                        var unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, "iteminfo.xml");
                        if (unzippedFile != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(Encoding.UTF8.GetString(unzippedFile.Bytes));
                            var esriInfo = doc["ESRI_ItemInformation"];

                            unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, $"{esriInfo["name"].InnerText}.info");
                            if (unzippedFile != null)
                            {
                                var jsonObject = this.serializer.Deserialize<dynamic>(Encoding.UTF8.GetString(unzippedFile.Bytes));

                                if (jsonObject?.maps?.Count > 0)
                                {
                                    var mapName = jsonObject.maps[0];
                                    unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, $"{mapName}.mmap");
                                    jsonObject = this.serializer.Deserialize<dynamic>(Encoding.UTF8.GetString(unzippedFile.Bytes));

                                    item.Wkid = WGS84Wkid;

                                    var extent = jsonObject.item.extent;

                                    item.XMinVal = extent[0][0];
                                    item.YMinVal = extent[0][1];
                                    item.XMaxVal = extent[1][0];
                                    item.YMaxVal = extent[1][1];

                                    var layers = jsonObject.map.baseMap.baseMapLayers;
                                    if (layers?.Count > 0)
                                    {
                                        var layer = layers[0];

                                        item.MaxScale = layer.maxScale ?? 0;
                                        item.MinScale = layer.minScale ?? 0;
                                    }

                                }
                            }
                        }
                    }
                    break;
                case ".tif":
                    {
                        try
                        {
                            var fullPath = Path.GetFullPath(tempFile);

                            var valueGdalHome = this.geospatialConfig.Value.GdalHome;
                            this.logger.LogInformation("Reading info from {FileName} with gdalinfo located in {GdalHome}", 
                                fullPath, valueGdalHome);
                            var startInfo = 
                                ConsoleCommand.Read(this.fileSystemAccessor.CombinePath(valueGdalHome, 
                                    "gdalinfo"),
                                    $"\"{fullPath}\" -json");
                            var deserialized = JsonConvert.DeserializeObject<GdalInfoOuput>(startInfo);

                            /*var allX = new List<double>
                            {
                                deserialized.CornerCoordinates.LowerLeft[0],
                                deserialized.CornerCoordinates.UpperLeft[0],
                                deserialized.CornerCoordinates.LowerRight[0],
                                deserialized.CornerCoordinates.UpperRight[0],
                            };

                            var allY = new List<double>
                            {
                                deserialized.CornerCoordinates.LowerLeft[1],
                                deserialized.CornerCoordinates.UpperLeft[1],
                                deserialized.CornerCoordinates.LowerRight[1],
                                deserialized.CornerCoordinates.UpperRight[1],
                            };
                            item.XMinVal = allX.Min();
                            item.YMinVal = allY.Min();

                            item.XMaxVal = allX.Max();
                            item.YMaxVal = allY.Max();

                            //item.Wkid = 32735; // probably written in WKT format in the deserialized.CoordinateSystem but there is no parser for it
                                 */

                            if (deserialized?.Wgs84Extent != null)
                            {
                                double xMin = double.MaxValue;
                                double xMax = double.MinValue;
                                double yMin = double.MaxValue;
                                double yMax = double.MinValue;

                                foreach (double[][] poli in deserialized.Wgs84Extent.Coordinates)
                                {
                                    foreach (double[] coord in poli)
                                    {
                                        xMin = Math.Min(xMin, coord[0]);
                                        xMax = Math.Max(xMax, coord[0]);

                                        yMin = Math.Min(yMin, coord[1]);
                                        yMax = Math.Max(yMax, coord[1]);
                                    }

                                }

                                item.XMinVal = xMin;
                                item.YMinVal = yMin;

                                item.XMaxVal = xMax;
                                item.YMaxVal = yMax;

                                item.Wkid = 4326; //geographic coordinates Wgs84
                            }
                            else
                                throw new Exception(".tif file is not recognized as map");
                        }
                        catch (Win32Exception e)
                        {
                            if (e.NativeErrorCode == 2)
                            {
                                throw new Exception("gdalinfo utility not found. Please install gdal library and add to PATH variable", e);
                            }
                        }
                        catch (NonZeroExitCodeException e)
                        {
                            if(!string.IsNullOrEmpty(e.ErrorOutput))
                                logger.LogError(e.ErrorOutput);
                            
                            if (e.ProcessExitCode == 4)
                            {
                                throw new Exception(".tif file is not recognized as map", e);
                            }

                            throw;
                        }
                    }
                    break;
            }

            return item;
        }

        public async Task<MapBrowseItem> DeleteMap(string mapName)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map != null)
                this.mapPlainStorageAccessor.Remove(mapName);

            if (externalFileStorage.IsEnabled())
            {
                this.logger.LogWarning("Deleting map: '{map}' from external storage", mapName);
                await this.externalFileStorage.RemoveAsync(GetExternalStoragePath(mapName));
            }
            else
            {
                this.logger.LogWarning("Deleting map: '{map}' from {folder}", mapName, this.mapsFolderPath);
                var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

                if (this.fileSystemAccessor.IsFileExists(filePath))
                    fileSystemAccessor.DeleteFile(filePath);
            }

            return map;
        }

        public async Task DeleteAllMaps()
        {
            this.logger.LogWarning("Deleting all maps execution started");
            var maps = await this.mapPlainStorageAccessor.Query(m => m.ToListAsync());
            
            foreach (var map in maps)
            {
                await this.DeleteMap(map.Id);
            }
        }

        public MapBrowseItem DeleteMapUserLink(string mapName, string user)
        {
            if (mapName == null) throw new ArgumentNullException(nameof(mapName));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lowerCasedUserName = user.ToLower();
            if (this.authorizedUser.IsSupervisor)
            {
                bool isTeamInterviewer = this.userStorage.Users
                    .Any(x => x.UserName.ToLower() == lowerCasedUserName && x.Profile.SupervisorId == this.authorizedUser.Id);
                if (!isTeamInterviewer)
                {
                    throw new UserNotFoundException("Map can be assigned only to existing non archived interviewer.");
                }
            }

            var map = this.mapPlainStorageAccessor.GetById(mapName);

            if (map == null)
                throw new Exception("Map was not found.");

            var mapUsers = this.userMapsStorage
                .Query(q => q.Where(x => x.Map.Id == mapName && x.UserName.ToLower() == lowerCasedUserName))
                .ToList();

            if (mapUsers.Count > 0)
                this.userMapsStorage.Remove(mapUsers);
            else
            {
                throw new Exception("Map is not assigned to specified interviewer.");
            }
            return map;
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
                    IQueryable<MapBrowseItem> pagedResults = queryable.OrderBy(x => x.FileName).Skip((page - 1) * pageSize).Take(pageSize);

                    itemIds = pagedResults.Select(x => x.Id).ToList();

                    return itemIds;
                });

                if (itemIds.Count < 1)
                    break;

                page++;

                List<Tuple<string, string>> maps = userMapsStorage.Query(q =>
                {
                    return q.Select(x => new Tuple<string, string>(x.Map.Id, x.UserName)).Where(x => itemIds.Contains(x.Item1))
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
                Headers = new[] { "map", "users" },
                Data = rows.ToArray()
            };
        }

        public void UpdateUserMaps(string mapName, string[] users)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map == null)
                throw new ArgumentException($"Map was not found {mapName}", nameof(mapName));

            var userMaps = userMapsStorage.Query(q => q.Where(x => x.Map.Id == mapName).ToList());

            var interviewerRoleId = UserRoles.Interviewer.ToUserId();
            var usersToLower = users.Select(em => em.ToLower()).ToList();

            var availableUsers = this.userStorage.Users
                .Where(x => usersToLower.Contains(x.UserName.ToLower()))
                .Select(x => new
                {
                    UserName = x.UserName,
                    IsArchived = x.IsArchived,
                    Roles = x.Roles
                }).ToArray();

            var userMappings = availableUsers
                .Where(y => y.IsArchived == false
                                               && y.Roles.Any(role => role.Id == interviewerRoleId))
                .Select(x => new UserMap() { Map = map, UserName = x.UserName }).ToList();

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
                        .Select(y => y.Map.Id)
                        .Distinct();
            }).ToArray();
        }

        public string[] GetAllMapsForInterviewer(string userName)
        {
            return userMapsStorage.Query(q =>
            {
                return q.Where(x => x.UserName == userName)
                        .Select(y => y.Map.Id)
                        .ToList();
            }).ToArray();
        }

        public MapBrowseItem GetMapById(string id) => this.mapPlainStorageAccessor.GetById(id);

        public MapBrowseItem AddUserToMap(string id, string userName)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (userName == null) throw new ArgumentNullException(nameof(userName));

            var map = this.mapPlainStorageAccessor.GetById(id);
            if (map == null)
                throw new Exception(@"Map was not found.");

            var userNameLowerCase = userName.ToLower();
            var interviewerRoleId = UserRoles.Interviewer.ToUserId();

            var userQuery = this.userStorage.Users
                .Where(x => x.UserName.ToLower() == userNameLowerCase &&
                            x.IsArchived == false &&
                            x.Roles.Any(role => role.Id == interviewerRoleId));
            if (authorizedUser.IsSupervisor)
            {
                userQuery = userQuery.Where(x => x.Profile.SupervisorId == this.authorizedUser.Id);
            }

            var interviewer = userQuery.FirstOrDefault();

            if (interviewer == null)
            {
                throw new UserNotFoundException("Map can be assigned only to existing non archived interviewer.");
            }

            var userMap = this.userMapsStorage
                .Query(x => x.FirstOrDefault(um => um.Map.FileName == id && um.UserName == userName));

            if (userMap != null)
            {
                throw new Exception("Provided map already assigned to specified interviewer.");
            }

            userMapsStorage.Store(new UserMap
            {
                UserName = userName,
                Map = map
            }, null);

            return this.mapPlainStorageAccessor.GetById(id);
        }

        public async Task<byte[]> GetMapContentAsync(string mapName)
        {
            if (externalFileStorage.IsEnabled())
            {
                return await this.externalFileStorage.GetBinaryAsync((GetExternalStoragePath(mapName)));
            }

            var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);

            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(filePath);
        }
    }
}
