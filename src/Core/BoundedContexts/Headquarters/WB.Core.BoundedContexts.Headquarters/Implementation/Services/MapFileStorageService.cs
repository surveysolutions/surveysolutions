using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DotSpatial.Projections;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Simplify;
using Newtonsoft.Json;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
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
        private readonly IPlainStorageAccessor<DuplicateMapLabel> duplicateMapLabelPlainStorageAccessor;
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
        private const string MapsFolderName = "MapsData";
        private const string LabelFieldName = "label";

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
            ILogger<MapFileStorageService> logger, 
            IPlainStorageAccessor<DuplicateMapLabel> duplicateMapLabelPlainStorageAccessor)
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
            this.duplicateMapLabelPlainStorageAccessor = duplicateMapLabelPlainStorageAccessor;
            this.geospatialConfig = geospatialConfig;

            this.mapsFolderPath = fileSystemAccessor.CombinePath(fileStorageConfig.Value.TempData, MapsFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.mapsFolderPath))
                fileSystemAccessor.CreateDirectory(this.mapsFolderPath);
        }

        public string GetExternalStoragePath(string name) => $"maps/" + name;

        public async Task<MapBrowseItem> SaveOrUpdateMapAsync(MapFiles mapFiles, string mapsDirectory)
        {
            string tempFile = null;
            try
            {
                var mapItem = this.ToMapBrowseItem(mapFiles, mapsDirectory);

                if (mapFiles.IsShapeFile)
                {
                    tempFile = fileSystemAccessor.CombinePath(mapsDirectory, mapFiles.Name + ".shp.zip");
                    var entities = mapFiles.Files.Select(f =>
                        fileSystemAccessor.CombinePath(mapsDirectory, f.Name)
                    );
                    archiveUtils.CreateArchiveFromFileList(entities, tempFile);
                }
                else
                {
                    tempFile = this.fileSystemAccessor.CombinePath(mapsDirectory, mapFiles.Name);
                }

                var mapName = mapFiles.IsShapeFile ? mapFiles.Name + ".shp" : mapFiles.Name; 
                if (externalFileStorage.IsEnabled())
                {
                    await using FileStream file = File.OpenRead(tempFile);
                    var name = this.fileSystemAccessor.GetFileName(mapName);
                    await this.externalFileStorage.StoreAsync(GetExternalStoragePath(name), file, "application/zip")
                        .ConfigureAwait(false);
                }
                else
                {
                    var targetFile = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, mapName);
                    fileSystemAccessor.MoveFile(tempFile, targetFile);
                }

                this.duplicateMapLabelPlainStorageAccessor.Remove(l => l.Where(d => d.Map.Id == mapItem.Id));
                this.mapPlainStorageAccessor.Store(mapItem, mapItem.Id);
                this.duplicateMapLabelPlainStorageAccessor.Store(mapItem.DuplicateLabels);
                return mapItem;
            }
            catch
            {
                if (this.fileSystemAccessor.IsFileExists(tempFile))
                    fileSystemAccessor.DeleteFile(tempFile);
                throw;
            }
        }

        private MapBrowseItem ToMapBrowseItem(MapFiles mapFile, string mapsDirectory)
        {
            var mapName = mapFile.IsShapeFile ? mapFile.Name + ".shp" : mapFile.Name; 
            var item = new MapBrowseItem
            {
                Id = mapName,
                ImportDate = DateTime.UtcNow,
                FileName = mapName,
                Size = mapFile.Size,
                Wkid = 102100,
                UploadedBy = authorizedUser.Id,
            };

            void SetMapProperties(dynamic _, MapBrowseItem i)
            {
                i.Wkid = _.fullExtent.spatialReference.wkid;
                i.XMaxVal = _.fullExtent.xmax;
                i.XMinVal = _.fullExtent.xmin;
                i.YMaxVal = _.fullExtent.ymax;
                i.YMinVal = _.fullExtent.ymin;
                i.MaxScale = _.maxScale;
                i.MinScale = _.minScale;
            };

            switch (this.fileSystemAccessor.GetFileExtension(mapName))
            {
                case ".tpk":
                    {
                        var tempFile = this.fileSystemAccessor.CombinePath(mapsDirectory, mapFile.Name);
                        var unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, "mapserver.json");
                        if (unzippedFile != null)
                        {
                            var jsonObject = this.serializer.Deserialize<dynamic>(Encoding.UTF8.GetString(unzippedFile.Bytes));
                            SetMapProperties(jsonObject.contents, item);
                        }
                        else
                        {
                            unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, "conf.cdi");
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
                        }
                    }
                    break;
                case ".mmpk":
                    {
                        var tempFile = this.fileSystemAccessor.CombinePath(mapsDirectory, mapFile.Name);
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

                                if (jsonObject != null && jsonObject.maps?.Count > 0)
                                {
                                    var jsonMapName = jsonObject.maps[0];
                                    unzippedFile = this.archiveUtils.GetFileFromArchive(tempFile, $"{jsonMapName}.mmap");
                                    jsonObject = this.serializer.Deserialize<dynamic>(Encoding.UTF8.GetString(unzippedFile.Bytes));

                                    item.Wkid = WGS84Wkid;

                                    var extent = jsonObject.item.extent;

                                    item.XMinVal = extent[0][0];
                                    item.YMinVal = extent[0][1];
                                    item.XMaxVal = extent[1][0];
                                    item.YMaxVal = extent[1][1];

                                    var layers = jsonObject.map.baseMap.baseMapLayers;
                                    if (layers != null && layers.Count > 0)
                                    {
                                        var layer = layers[0];

                                        item.MaxScale = layer.maxScale ?? 0;
                                        item.MinScale = layer.minScale ?? 0;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException(".mmpk file has no base layers");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid data. iteminfo.xml not found");
                        }
                    }
                    break;
                case ".tif":
                    {
                        var tempFile = Path.Combine(mapsDirectory, mapFile.Name);
                        var fullPath = Path.GetFullPath(tempFile);

                        bool isReaded = TryReadGdalInfomation(fullPath, out var deserialized);

                        if (isReaded)
                        {
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

                                item.Wkid = WGS84Wkid; //geographic coordinates Wgs84
                            }
                            else
                                throw new InvalidOperationException(".tif file is not recognized as map");
                        }
                        else
                        {
                            var isGeoTiff = GeoTiffInfoReader.IsGeoTIFF(fullPath);
                            if (!isGeoTiff)
                                throw new InvalidOperationException(".tif file is not recognized as map");    
                        }
                    }
                    break;
                case ".shp": // shape file
                {
                    try
                    {
                        var shapeFilePath = fileSystemAccessor.CombinePath(mapsDirectory, mapName);

                        using var shapefileReader = Shapefile.OpenRead(shapeFilePath);
                       
                        var headerBounds = shapefileReader.BoundingBox;
                        item.XMinVal = headerBounds.MinX;
                        item.YMinVal = headerBounds.MinY;
                        item.XMaxVal = headerBounds.MaxX;
                        item.YMaxVal = headerBounds.MaxY;
                        item.ShapeType = shapefileReader.ShapeType.ToString();
                        item.Wkid = 4326;  //geographic coordinates Wgs84
                        item.ShapesCount = shapefileReader.RecordCount;
                        
                        int? labelIndexOf = null;

                        for (int i = 0; i < shapefileReader.Fields.Count; i++)
                        {
                            if (shapefileReader.Fields[i].Name == LabelFieldName)
                            {
                                labelIndexOf = i + 1;
                                break;
                            }
                        }
                        
                        CoordinateTransformationFilter coordinateTransformationFilter = null;
                        var projection = shapefileReader.Projection;
                        var sourceProjectionInfo = ProjectionInfo.FromEsriString(projection);
                        if (!sourceProjectionInfo.IsLatLon)
                        {
                            var target = KnownCoordinateSystems.Geographic.World.WGS1984;
                            coordinateTransformationFilter = new CoordinateTransformationFilter(sourceProjectionInfo, target);

                            var minHeaderCoordinate = coordinateTransformationFilter.Transform(headerBounds.MinX, headerBounds.MinY);
                            item.XMinVal = minHeaderCoordinate.X;
                            item.YMinVal = minHeaderCoordinate.Y;
                            var maxHeaderCoordinate = coordinateTransformationFilter.Transform(headerBounds.MaxX, headerBounds.MaxY);
                            item.XMaxVal = maxHeaderCoordinate.X;
                            item.YMaxVal = maxHeaderCoordinate.Y;
                        }
                        
                        FeatureCollection fc = new FeatureCollection();
                        HashSet<string> checkOnUnique = new HashSet<string>();
                        Dictionary<string, int> duplicateLabels = new Dictionary<string, int>();

                        while (shapefileReader.Read(out bool deleted, out var readFeature))
                        {
                            AttributesTable attribs = new AttributesTable();

                            if (labelIndexOf.HasValue)
                            {
                                var labelValue = readFeature.Attributes[LabelFieldName].ToString();

                                if (!string.IsNullOrWhiteSpace(labelValue))
                                {
                                    attribs.Add(LabelFieldName, labelValue);

                                    if (!checkOnUnique.Add(labelValue))
                                    {
                                        if (!duplicateLabels.TryAdd(labelValue, 2))
                                            duplicateLabels[labelValue] += 1;
                                    }
                                }
                            }

                            var featureGeometry = Transform(readFeature.Geometry, coordinateTransformationFilter);
                            IFeature feature = new Feature(featureGeometry, attribs);
                            fc.Add(feature);
                        }

                        if (fc.Count == 0)
                            throw new ArgumentException($"Can't read any coordinates from {mapFile.Name}.shp file");

                        item.DuplicateLabels.Clear();
                        foreach (var duplicateLabel in duplicateLabels)
                        {
                            item.DuplicateLabels.Add(new DuplicateMapLabel()
                            {
                                Label = duplicateLabel.Key,
                                Count = duplicateLabel.Value,
                                Map = item,
                            });
                        }

                        var json = GetGeoJson(fc);
                        var byteCount = Encoding.Unicode.GetByteCount(json);
                        if (byteCount > geospatialConfig.Value.GeoJsonMaxSize)
                        {
                            UnaryUnionOp c = new UnaryUnionOp(fc.Select(f => f.Geometry).ToArray());
                            var geometryCollection = c.Union();
                            //DouglasPeuckerSimplifier simplifier = new DouglasPeuckerSimplifier(geometryCollection);
                            TopologyPreservingSimplifier simplifier = new TopologyPreservingSimplifier(geometryCollection);
                            var simplifierGeometry = simplifier.GetResultGeometry();

                            FeatureCollection unionFc = new FeatureCollection();
                            unionFc.Add(new Feature(simplifierGeometry, new AttributesTable()));
                            json = GetGeoJson(unionFc);
                            item.IsPreviewGeoJson = true;
                        }

                        item.GeoJson = json;
                    }
                    catch (ArgumentException ex)
                    {
                        logger.LogError(ex, "Can't read {MapName}.shp file", mapFile.Name);
                        throw;
                    }                    
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Can't read {MapName}.shp file", mapFile.Name);
                        throw new InvalidOperationException($"Can't read {mapFile.Name}.shp file. File is corrupt.", ex);
                    }
                }
                break;
            }

            return item;
        }

        private bool TryReadGdalInfomation(string fullPath, out GdalInfoOuput gdalInfo)
        {
            gdalInfo = null;
            
            var valueGdalHome = this.geospatialConfig.Value.GdalHome;

            if (string.IsNullOrWhiteSpace(valueGdalHome))
                return false;
                
            try
            {
                this.logger.LogInformation("Reading info from {FileName} with gdalinfo located in {GdalHome}", 
                    fullPath, valueGdalHome);
            
                var startInfo = ConsoleCommand.Read(Path.Combine(valueGdalHome, "gdalinfo"), $"\"{fullPath}\" -json");
                gdalInfo = JsonConvert.DeserializeObject<GdalInfoOuput>(startInfo);

                return true;
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == 2)
                {
                    //throw new InvalidOperationException("gdalinfo utility not found. Please install gdal library and add to PATH variable", e);
                    return false;
                }
            }
            catch (NonZeroExitCodeException e)
            {
                if(!string.IsNullOrEmpty(e.ErrorOutput))
                    logger.LogError(e.ErrorOutput);
                
                if (e.ProcessExitCode == 4)
                {
                    throw new InvalidOperationException(".tif file is not recognized as map", e);
                }

                throw;
            }
            
            return false;
        }

        private Geometry Transform(Geometry geometry, CoordinateTransformationFilter coordinateTransformation)
        {
            if (coordinateTransformation == null)
                return geometry;
            
            geometry.Apply(coordinateTransformation);
            return geometry;
        }

        private static string GetGeoJson(FeatureCollection fc)
        {
            var jsonSerializer = GeoJsonSerializer.Create();
            StringBuilder sb = new StringBuilder();
            using TextWriter tw = new StringWriter(sb);
            jsonSerializer.Serialize(tw, fc);
            tw.Flush();
            var json = sb.ToString();
            return json;
        }

        public async Task<MapBrowseItem> DeleteMap(string mapName)
        {
            var fileName = fileSystemAccessor.GetFileName(mapName);
            var map = await this.mapPlainStorageAccessor.GetByIdAsync(fileName);
            if (map == null)
                throw new Exception("Map was not found.");

            this.mapPlainStorageAccessor.Remove(map.FileName);

            if (externalFileStorage.IsEnabled())
            {
                this.logger.LogWarning("Deleting map: '{map}' from external storage", map.FileName);
                await this.externalFileStorage.RemoveAsync(GetExternalStoragePath(map.FileName));
            }
            else
            {
                this.logger.LogWarning("Deleting map: '{map}' from {folder}", map.FileName, this.mapsFolderPath);
                var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, map.FileName);

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
                await this.DeleteMap(map.FileName);
            }
        }

        public MapBrowseItem DeleteMapUserLink(string mapName, string user)
        {
            if (mapName == null) throw new ArgumentNullException(nameof(mapName));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var fileName = fileSystemAccessor.GetFileName(mapName);
            var map = this.mapPlainStorageAccessor.GetById(fileName);
            if (map == null)
                throw new Exception("Map was not found.");
            
            var lowerCasedUserName = user.ToLower();
            if (this.authorizedUser.IsSupervisor)
            {
                bool isTeamInterviewer = this.userStorage.Users
                    .Any(x => x.UserName.ToLower() == lowerCasedUserName 
                              && x.WorkspaceProfile.SupervisorId == this.authorizedUser.Id);

                bool isSelf =  !string.IsNullOrEmpty(this.authorizedUser.UserName) 
                               && this.authorizedUser.UserName.ToLower() == lowerCasedUserName;
                
                if (!isTeamInterviewer && !isSelf)
                {
                    throw new UserNotFoundException("User is not from the team.");
                }
            }
            

            var mapUsers = this.userMapsStorage
                .Query(q => q.Where(x => x.Map.Id == map.FileName && x.UserName.ToLower() == lowerCasedUserName))
                .ToList();

            if (mapUsers.Count > 0)
                this.userMapsStorage.Remove(mapUsers);
            else
            {
                throw new InvalidOperationException("Map is not assigned to specified user.");
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
            var fileName = fileSystemAccessor.GetFileName(mapName);
            var map = this.mapPlainStorageAccessor.GetById(fileName);
            if (map == null)
                throw new ArgumentException($"Map was not found {mapName}", nameof(mapName));

            var userMaps = userMapsStorage.Query(q => q.Where(x => x.Map.Id == map.FileName).ToList());

            var interviewerRoleId = UserRoles.Interviewer.ToUserId();
            var supervisorRoleId = UserRoles.Supervisor.ToUserId();
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
                                               && y.Roles.Any(role => role.Id == interviewerRoleId || role.Id == supervisorRoleId))
                .Select(x => new UserMap() { Map = map, UserName = x.UserName }).ToList();

            userMapsStorage.Remove(userMaps);
            userMapsStorage.Store(userMappings.Select(x => Tuple.Create(x, (object)x)));
        }

        public string[] GetAllMapsForSupervisor(Guid supervisorId)
        {
            var interviewerNames = this.userStorage.Users
                .Where(x => (supervisorId == x.WorkspaceProfile.SupervisorId || supervisorId == x.Id) && x.IsArchived == false)
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

        public MapBrowseItem GetMapById(string id) => this.mapPlainStorageAccessor.GetById(fileSystemAccessor.GetFileName(id));

        public MapBrowseItem AddUserToMap(string id, string userName)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (userName == null) throw new ArgumentNullException(nameof(userName));

            var fileName = fileSystemAccessor.GetFileName(id);
            var map = this.mapPlainStorageAccessor.GetById(fileName);
            if (map == null)
                throw new InvalidOperationException(@"Map was not found.");

            var userNameLowerCase = userName.ToLower();
            var interviewerRoleId = UserRoles.Interviewer.ToUserId();
            var supervisorRoleId = UserRoles.Supervisor.ToUserId();
            
            var userQuery = this.userStorage.Users
                .Where(x => x.UserName.ToLower() == userNameLowerCase &&
                            x.IsArchived == false &&
                            x.Roles.Any(role => role.Id == interviewerRoleId || role.Id == supervisorRoleId));
            if (authorizedUser.IsSupervisor)
            {
                var supervisorId = this.authorizedUser.Id;
                userQuery = userQuery.Where(x => x.WorkspaceProfile.SupervisorId == supervisorId
                                                    || x.Id == supervisorId);
            }

            var user = userQuery.FirstOrDefault();

            if (user == null)
            {
                throw new UserNotFoundException("Map can be assigned only to existing non archived interviewer or supervisor.");
            }

            var userMap = this.userMapsStorage
                .Query(x => x.FirstOrDefault(um => um.Map.FileName == map.FileName && um.UserName == userName));

            if (userMap == null)
            {
                userMapsStorage.Store(new UserMap
                {
                    UserName = userName,
                    Map = map
                }, null);
            }

            return map;
        }

        public ComboboxViewItem[] GetUserShapefiles(string filter)
        {
            return mapPlainStorageAccessor.Query(query =>
            {
                if (authorizedUser.IsSupervisor)
                {
                    var supervisorId = authorizedUser.Id;
                    var userNames = this.userStorage.Users
                        .Where(x => (supervisorId == x.WorkspaceProfile.SupervisorId || supervisorId == x.Id) && x.IsArchived == false)
                        .Select(x => x.UserName)
                        .ToArray();

                    query = query.Where(x => x.Users.Any(u => userNames.Contains(u.UserName)));
                }

                if (authorizedUser.IsInterviewer)
                {
                    var userName = authorizedUser.UserName;
                    query = query.Where(x => x.Users.Any(u => u.UserName == userName));
                }

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query = query.Where(x => x.FileName.Contains(filter));
                }

                return query
                    .Where(x => x.GeoJson != null)
                    .Select(x => new { x.Id, x.FileName })
                    .OrderBy(x => x.FileName)
                    .Select(x => new ComboboxViewItem(x.FileName, x.Id, null));

            }).ToArray();
        }

        public async Task<byte[]> GetMapContentAsync(string mapName)
        {
            var fileName = fileSystemAccessor.GetFileName(mapName);
            var map = await this.mapPlainStorageAccessor.GetByIdAsync(fileName);
            if (map == null)
                throw new InvalidOperationException(@"Map was not found.");

            if (externalFileStorage.IsEnabled())
            {
                return await this.externalFileStorage.GetBinaryAsync(GetExternalStoragePath(map.FileName));
            }

            var filePath = this.fileSystemAccessor.CombinePath(this.mapsFolderPath, map.FileName);
            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;

            return this.fileSystemAccessor.ReadAllBytes(filePath);
        }
    }
}
