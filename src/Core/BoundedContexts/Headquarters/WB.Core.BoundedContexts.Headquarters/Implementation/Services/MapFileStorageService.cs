using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using OSGeo.GDAL;
using OSGeo.OGR;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Storage;
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

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public static partial class GdalConfiguration
    {
        private static volatile bool _configuredOgr;
        private static volatile bool _configuredGdal;
        private static volatile bool _usable;

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDefaultDllDirectories(uint directoryFlags);
        //               LOAD_LIBRARY_SEARCH_USER_DIRS | LOAD_LIBRARY_SEARCH_SYSTEM32
        private const uint DllSearchFlags = 0x00000400 | 0x00000800;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddDllDirectory(string lpPathName);

        /// <summary>
        /// Construction of Gdal/Ogr
        /// </summary>
        static GdalConfiguration()
        {
            string executingDirectory = null, gdalPath = null, nativePath = null;
            try
            {
                if (!IsWindows)
                {
                    const string notSet = "_Not_set_";
                    string tmp = Gdal.GetConfigOption("GDAL_DATA", notSet);
                    _usable = tmp != notSet;
                    return;
                }

                string executingAssemblyFile = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath;
                executingDirectory = Path.GetDirectoryName(executingAssemblyFile);

                if (string.IsNullOrEmpty(executingDirectory))
                    throw new InvalidOperationException("cannot get executing directory");


                // modify search place and order
                SetDefaultDllDirectories(DllSearchFlags);

                gdalPath = Path.Combine(executingDirectory, "gdal");
                nativePath = Path.Combine(gdalPath, GetPlatform());
                if (!Directory.Exists(nativePath))
                    throw new DirectoryNotFoundException($"GDAL native directory not found at '{nativePath}'");
                if (!File.Exists(Path.Combine(nativePath, "gdal_wrap.dll")))
                    throw new FileNotFoundException(
                        $"GDAL native wrapper file not found at '{Path.Combine(nativePath, "gdal_wrap.dll")}'");

                // Add directories
                AddDllDirectory(nativePath);
                AddDllDirectory(Path.Combine(nativePath, "plugins"));

                // Set the additional GDAL environment variables.
                string gdalData = Path.Combine(gdalPath, "data");
                Environment.SetEnvironmentVariable("GDAL_DATA", gdalData);
                Gdal.SetConfigOption("GDAL_DATA", gdalData);

                string driverPath = Path.Combine(nativePath, "plugins");
                Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", driverPath);
                Gdal.SetConfigOption("GDAL_DRIVER_PATH", driverPath);

                Environment.SetEnvironmentVariable("GEOTIFF_CSV", gdalData);
                Gdal.SetConfigOption("GEOTIFF_CSV", gdalData);

                string projSharePath = Path.Combine(gdalPath, "share");
                Environment.SetEnvironmentVariable("PROJ_LIB", projSharePath);
                Gdal.SetConfigOption("PROJ_LIB", projSharePath);

                _usable = true;
            }
            catch (Exception e)
            {
                _usable = false;
                Trace.WriteLine(e, "error");
                Trace.WriteLine($"Executing directory: {executingDirectory}", "error");
                Trace.WriteLine($"gdal directory: {gdalPath}", "error");
                Trace.WriteLine($"native directory: {nativePath}", "error");

                //throw;
            }
        }

        /// <summary>
        /// Gets a value indicating if the GDAL package is set up properly.
        /// </summary>
        public static bool Usable
        {
            get { return _usable; }
        }

        /// <summary>
        /// Method to ensure the static constructor is being called.
        /// </summary>
        /// <remarks>Be sure to call this function before using Gdal/Ogr/Osr</remarks>
        public static void ConfigureOgr()
        {
            if (!_usable) return;
            if (_configuredOgr) return;

            // Register drivers
            Ogr.RegisterAll();
            _configuredOgr = true;

            PrintDriversOgr();
        }

        /// <summary>
        /// Method to ensure the static constructor is being called.
        /// </summary>
        /// <remarks>Be sure to call this function before using Gdal/Ogr/Osr</remarks>
        public static void ConfigureGdal()
        {
            if (!_usable) return;
            if (_configuredGdal) return;

            // Register drivers
            Gdal.AllRegister();
            _configuredGdal = true;

            PrintDriversGdal();
        }


        /// <summary>
        /// Function to determine which platform we're on
        /// </summary>
        private static string GetPlatform()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        /// <summary>
        /// Gets a value indicating if we are on a windows platform
        /// </summary>
        private static bool IsWindows
        {
            get
            {
                var res = !(Environment.OSVersion.Platform == PlatformID.Unix ||
                            Environment.OSVersion.Platform == PlatformID.MacOSX);

                return res;
            }
        }
        private static void PrintDriversOgr()
        {
#if DEBUG
            if (_usable)
            {
                var num = Ogr.GetDriverCount();
                for (var i = 0; i < num; i++)
                {
                    var driver = Ogr.GetDriver(i);
                    Trace.WriteLine($"OGR {i}: {driver.GetName()}", "Debug");
                }
            }
#endif
        }

        private static void PrintDriversGdal()
        {
#if DEBUG
            if (_usable)
            {
                var num = Gdal.GetDriverCount();
                for (var i = 0; i < num; i++)
                {
                    var driver = Gdal.GetDriver(i);
                    Trace.WriteLine($"GDAL {i}: {driver.ShortName}-{driver.LongName}");
                }
            }
#endif
        }
    }

    public class MapFileStorageService : IMapStorageService
    {
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;
        private readonly IPlainStorageAccessor<UserMap> userMapsStorage;
        private readonly ISerializer serializer;
        private readonly IUserRepository userStorage;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;

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
            IExternalFileStorage externalFileStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.userMapsStorage = userMapsStorage;
            this.serializer = serializer;
            this.userStorage = userStorage;

            this.externalFileStorage = externalFileStorage;

            this.path = fileSystemAccessor.CombinePath(fileStorageConfig.Value.AppData, TempFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            this.mapsFolderPath = fileSystemAccessor.CombinePath(fileStorageConfig.Value.AppData, MapsFolderName);
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
                var mapItem = this.ToMapBrowseItem(tempFile, mapFile);

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

                                item.Wkid = jsonObject.map.spatialReference.wkid;

                                var extent = jsonObject.item.extent;

                                item.XMinVal = extent[0][0];
                                item.YMinVal = extent[0][1];
                                item.XMaxVal = extent[1][0];
                                item.YMaxVal = extent[1][1];

                                var layers = jsonObject.map.baseMap.baseMapLayers;
                                if (layers?.Count > 0)
                                {
                                    var layer = layers[0];

                                    item.MaxScale = layer.maxScale;
                                    item.MinScale = layer.minScale;
                                }

                            }
                        }
                    }
                }
                    break;
                case ".tif":
                {
                    GdalConfiguration.ConfigureGdal();
                    using (Dataset ds = Gdal.Open(tempFile, Access.GA_ReadOnly))
                    using (SpatialReference sr = new SpatialReference(ds.GetProjection()))
                    {
                        if (sr.IsProjected() < 1)
                            throw new ArgumentException(
                                $"Geotiff is not projected. {this.fileSystemAccessor.GetFileName(tempFile)}");

                        if (int.TryParse(sr.GetAuthorityCode(null), out var wkid))
                            item.Wkid = wkid == 0 ? item.Wkid : wkid;

                        double[] geoTransform = new double[6];
                        ds.GetGeoTransform(geoTransform);

                        item.XMinVal = geoTransform[0];
                        item.YMaxVal = geoTransform[3];
                        item.XMaxVal = item.XMinVal + geoTransform[1] * ds.RasterXSize;
                        item.YMinVal = item.YMaxVal + geoTransform[5] * ds.RasterYSize;
                    }
                }
                    break;
            }

            return item;
        }

        public async Task DeleteMap(string mapName)
        {
            var map = this.mapPlainStorageAccessor.GetById(mapName);
            if (map != null)
                this.mapPlainStorageAccessor.Remove(mapName);

            if (externalFileStorage.IsEnabled())
            {
                await this.externalFileStorage.RemoveAsync(GetExternalStoragePath(mapName));
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
