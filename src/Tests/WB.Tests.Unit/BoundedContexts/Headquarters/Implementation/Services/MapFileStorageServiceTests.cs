using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Configs;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Implementation.Services
{
    [TestOf(typeof(MapFileStorageService))]
    public class MapFileStorageServiceTests
    {
        [Test]
        public void AddUserToMap_Should_add_user_to_map()
        {
            var userName = "interviewer";
            var mapName = "mapFile.tpk";
            
            var user = Create.Entity.HqUser(userName: userName, role: UserRoles.Interviewer);

            var users = Create.Storage.UserRepository(user);
            var userMapStorage = new TestPlainStorage<UserMap>();
                
            var mapStorage = new TestPlainStorage<MapBrowseItem>();
            mapStorage.Store(Create.Entity.MapBrowseItem(mapName), mapName);
            
            var service = Create.Service.MapFileStorageService(userStorage: users, 
                userMapsStorage: userMapStorage,
                mapsStorage: mapStorage);
            
            // Act
            
            service.AddUserToMap(mapName, userName);
            
            // Assert
            var storedUserMap = userMapStorage.Query(_ => _.FirstOrDefault(um => um.UserName == userName));

            Assert.That(storedUserMap, Is.Not.Null);
        }

        [Test]
        public void AddUserToMap_when_called_by_supervisor_should_only_allow_to_assign_map_for_his_team()
        {
            var userName = "interviewer";
            var mapName = "mapFile.tpk";
            var userNameFromTeam = userName + "1";
            
            var user = Create.Entity.HqUser(userName: userName, 
                role: UserRoles.Interviewer,
                supervisorId: Id.gA);
            
            var userFromHisTeam = Create.Entity.HqUser(userName: userNameFromTeam, 
                role: UserRoles.Interviewer,
                supervisorId: Id.gB);

            var authorizedUser = Mock.Of<IAuthorizedUser>(
                u => u.IsSupervisor == true &&
                     u.Id == Id.gB
                );

            var users = Create.Storage.UserRepository(user, userFromHisTeam);
            var userMapStorage = new TestPlainStorage<UserMap>();
                
            var mapStorage = new TestPlainStorage<MapBrowseItem>();
            mapStorage.Store(Create.Entity.MapBrowseItem(mapName), mapName);
            
            var service = Create.Service.MapFileStorageService(userStorage: users, 
                userMapsStorage: userMapStorage,
                mapsStorage: mapStorage,
                authorizedUser: authorizedUser);
            
            // Act
            TestDelegate act = () => service.AddUserToMap(mapName, userName);
            service.AddUserToMap(mapName, userNameFromTeam);
            
            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<UserNotFoundException>());
            var notStoredUserMap = userMapStorage.Query(_ => _.FirstOrDefault(um => um.UserName == userName));
            Assert.That(notStoredUserMap, Is.Null);

            var storedUserMap = userMapStorage.Query(_ => _.FirstOrDefault(um => um.UserName == userNameFromTeam));
            Assert.That(storedUserMap, Is.Not.Null);
        }

        [Test]
        public void DeleteMapUserLink_when_called_by_supervisor_should_allow_deleting_map_only_from_team_member()
        {
            var userName = "interviewer";
            var mapName = "mapFile.tpk";
            var userNameFromTeam = userName + "1";
            
            var user = Create.Entity.HqUser(userName: userName, 
                role: UserRoles.Interviewer,
                supervisorId: Id.gA);
            
            var userFromHisTeam = Create.Entity.HqUser(userName: userNameFromTeam, 
                role: UserRoles.Interviewer,
                supervisorId: Id.gB);

            var authorizedUser = Mock.Of<IAuthorizedUser>(
                u => u.IsSupervisor == true &&
                     u.Id == Id.gB && u.UserName == "supervisor"
            );

            var users = Create.Storage.UserRepository(user, userFromHisTeam);
            
            var mapStorage = new TestPlainStorage<MapBrowseItem>();
            var mapBrowseItem = Create.Entity.MapBrowseItem(mapName);
            mapStorage.Store(mapBrowseItem, mapName);
            
            var userMapStorage = new TestPlainStorage<UserMap>();
            userMapStorage.Store(new UserMap
            {
                UserName = userName,
                Map = mapBrowseItem,
                Id = 1
            }, 1);    
            userMapStorage.Store(new UserMap
            {
                UserName = userNameFromTeam,
                Map = mapBrowseItem,
                Id = 2
            }, 2);    
            
            var service = Create.Service.MapFileStorageService(userStorage: users, 
                userMapsStorage: userMapStorage,
                mapsStorage: mapStorage,
                authorizedUser: authorizedUser);
            
            // Act
            TestDelegate act = () => service.DeleteMapUserLink(mapName, userName);
            service.DeleteMapUserLink(mapName, userNameFromTeam);
            
            // Assert
            Assert.That(act, Throws.Exception.InstanceOf<UserNotFoundException>());
            var notStoredUserMap = userMapStorage.Query(_ => _.FirstOrDefault(um => um.UserName == userName));
            Assert.That(notStoredUserMap, Is.Not.Null);

            var storedUserMap = userMapStorage.Query(_ => _.FirstOrDefault(um => um.UserName == userNameFromTeam));
            Assert.That(storedUserMap, Is.Null);
        }

        [Test]
        public async Task DeleteMap_should_store_a_pending_file_deletion_request()
        {
            const string mapName = "mapFile.tpk";
            var mapsStorage = new TestPlainStorage<MapBrowseItem>();
            mapsStorage.Store(Create.Entity.MapBrowseItem(mapName), mapName);
            var deletionRequests = new TestPlainStorage<MapFileDeletionRequest>();
            var service = Create.Service.MapFileStorageService(
                mapsStorage: mapsStorage,
                mapFileDeletionRequests: deletionRequests);

            await service.DeleteMap(mapName);

            Assert.That(mapsStorage.GetById(mapName), Is.Null);
            Assert.That(deletionRequests.GetById(mapName)?.FileName, Is.EqualTo(mapName));
        }

        [Test]
        public async Task DeleteMap_should_acquire_file_lock_before_loading_map_metadata()
        {
            const string mapName = "mapFile.tpk";
            var calls = new List<string>();
            var map = Create.Entity.MapBrowseItem(mapName);
            var mapsStorage = new Mock<IPlainStorageAccessor<MapBrowseItem>>();
            mapsStorage.Setup(storage => storage.GetByIdAsync(mapName))
                .Callback<object>(_ => calls.Add("load"))
                .ReturnsAsync(map);
            var deletionRequests = new TestPlainStorage<MapFileDeletionRequest>();
            var sqlQuery = new Mock<ISQLQuery>();
            sqlQuery.Setup(query => query.SetString("fileName", mapName)).Returns(sqlQuery.Object);
            sqlQuery.Setup(query => query.ExecuteUpdateAsync(It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("lock"))
                .ReturnsAsync(1);
            var session = new Mock<ISession>();
            session.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(sqlQuery.Object);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(x => x.Session).Returns(session.Object);
            var service = Create.Service.MapFileStorageService(
                mapsStorage: mapsStorage.Object,
                mapFileDeletionRequests: deletionRequests,
                unitOfWork: unitOfWork.Object);

            await service.DeleteMap(mapName);

            Assert.That(calls, Is.EqualTo(new[] { "lock", "load" }));
        }

        [Test]
        public async Task DeleteAllMaps_should_delete_files_without_queuing_cleanup_requests()
        {
            const string mapName = "mapFile.tpk";
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempBase);
            try
            {
                var mapsStorage = new TestPlainStorage<MapBrowseItem>();
                mapsStorage.Store(Create.Entity.MapBrowseItem(mapName), mapName);
                var deletionRequests = new TestPlainStorage<MapFileDeletionRequest>();
                deletionRequests.Store(new MapFileDeletionRequest { Id = "deleted-map.tpk", FileName = "deleted-map.tpk" }, "deleted-map.tpk");
                var mapsAccessor = new Mock<IPlainStorageAccessor<MapBrowseItem>>();
                mapsAccessor
                    .Setup(accessor => accessor.Query(It.IsAny<Func<IQueryable<MapBrowseItem>, Task<List<MapBrowseItem>>>>()))
                    .Returns((Func<IQueryable<MapBrowseItem>, Task<List<MapBrowseItem>>> query) =>
                        query(new TestingQueryable<MapBrowseItem>(mapsStorage.Query(items => items))));
                mapsAccessor.Setup(accessor => accessor.Remove(It.IsAny<object>()))
                    .Callback<object>(mapsStorage.Remove);
                var deletionRequestsAccessor = new Mock<IPlainStorageAccessor<MapFileDeletionRequest>>();
                deletionRequestsAccessor
                    .Setup(accessor => accessor.Query(It.IsAny<Func<IQueryable<MapFileDeletionRequest>, Task<List<MapFileDeletionRequest>>>>()))
                    .Returns((Func<IQueryable<MapFileDeletionRequest>, Task<List<MapFileDeletionRequest>>> query) =>
                        query(new TestingQueryable<MapFileDeletionRequest>(deletionRequests.Query(items => items))));
                deletionRequestsAccessor.Setup(accessor => accessor.Remove(It.IsAny<IEnumerable<MapFileDeletionRequest>>()))
                    .Callback<IEnumerable<MapFileDeletionRequest>>(deletionRequests.Remove);
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapsAccessor.Object,
                    mapFileDeletionRequests: deletionRequestsAccessor.Object,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = tempBase }));
                var filePath = Path.Combine(tempBase, "MapsData", mapName);
                var pendingFilePath = Path.Combine(tempBase, "MapsData", "deleted-map.tpk");
                File.WriteAllText(filePath, "map");
                File.WriteAllText(pendingFilePath, "pending map");

                await service.DeleteAllMaps();

                Assert.That(mapsStorage.GetById(mapName), Is.Null);
                Assert.That(File.Exists(filePath), Is.False);
                Assert.That(deletionRequests.GetById(mapName), Is.Null);
                Assert.That(File.Exists(pendingFilePath), Is.False);
                Assert.That(deletionRequests.GetById("deleted-map.tpk"), Is.Null);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        [Test]
        public async Task RemoveMapFileIfNotInUseAsync_when_map_exists_should_keep_its_file()
        {
            const string mapName = "mapFile.tpk";
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempBase);
            try
            {
                var mapsStorage = new TestPlainStorage<MapBrowseItem>();
                mapsStorage.Store(Create.Entity.MapBrowseItem(mapName), mapName);
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapsStorage,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = tempBase }));
                var filePath = Path.Combine(tempBase, "MapsData", mapName);
                File.WriteAllText(filePath, "map");

                var deleted = await service.RemoveMapFileIfNotInUseAsync(mapName);

                Assert.That(deleted, Is.False);
                Assert.That(File.Exists(filePath), Is.True);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        [Test]
        public async Task SaveOrUpdateMapAsync_when_shapefile_has_duplicate_label_values_HasDuplicateLabels_is_true()
        {
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var mapsDirectory = Path.Combine(tempBase, "source");
            var storageDirectory = Path.Combine(tempBase, "storage");
            Directory.CreateDirectory(mapsDirectory);
            Directory.CreateDirectory(storageDirectory);

            try
            {
                const string mapName = "testmap";
                WriteShapefileWithLabels(mapsDirectory, mapName, new[] { "region1", "region1", "region2" });

                var mapStorage = new TestPlainStorage<MapBrowseItem>();
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapStorage,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = storageDirectory }),
                    geospatialConfig: Options.Create(new GeospatialConfig())
                );

                // Act
                var result = await service.SaveOrUpdateMapAsync(BuildShapeMapFiles(mapName), mapsDirectory);

                // Assert
                Assert.That(result.HasDuplicateLabels, Is.True);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        [Test]
        public async Task SaveOrUpdateMapAsync_when_shapefile_has_unique_label_values_HasDuplicateLabels_is_false()
        {
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var mapsDirectory = Path.Combine(tempBase, "source");
            var storageDirectory = Path.Combine(tempBase, "storage");
            Directory.CreateDirectory(mapsDirectory);
            Directory.CreateDirectory(storageDirectory);

            try
            {
                const string mapName = "testmap";
                WriteShapefileWithLabels(mapsDirectory, mapName, new[] { "region1", "region2", "region3" });

                var mapStorage = new TestPlainStorage<MapBrowseItem>();
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapStorage,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = storageDirectory }),
                    geospatialConfig: Options.Create(new GeospatialConfig())
                );

                // Act
                var result = await service.SaveOrUpdateMapAsync(BuildShapeMapFiles(mapName), mapsDirectory);

                // Assert
                Assert.That(result.HasDuplicateLabels, Is.False);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        [Test]
        public async Task SaveOrUpdateMapAsync_when_tiff_has_no_readable_coordinates_imports_map_without_bounds()
        {
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var mapsDirectory = Path.Combine(tempBase, "source");
            var storageDirectory = Path.Combine(tempBase, "storage");
            var mapsFolder = Path.Combine(tempBase, "maps");
            Directory.CreateDirectory(mapsDirectory);
            Directory.CreateDirectory(storageDirectory);
            Directory.CreateDirectory(mapsFolder);

            try
            {
                const string mapName = "plainmap.tif";
                WritePlainTiff(Path.Combine(mapsDirectory, mapName));

                var mapStorage = new TestPlainStorage<MapBrowseItem>();
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapStorage,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = mapsFolder }),
                    geospatialConfig: Options.Create(new GeospatialConfig())
                );

                // Act — a georeferencing-less TIFF must be accepted, not rejected
                var result = await service.SaveOrUpdateMapAsync(BuildTiffMapFiles(mapName), mapsDirectory);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(mapName));
                Assert.That(result.XMinVal, Is.EqualTo(0));
                Assert.That(result.YMinVal, Is.EqualTo(0));
                Assert.That(result.XMaxVal, Is.EqualTo(0));
                Assert.That(result.YMaxVal, Is.EqualTo(0));
                Assert.That(mapStorage.GetById(mapName), Is.Not.Null);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        [Test]
        public void SaveOrUpdateMapAsync_when_tif_file_is_not_a_valid_tiff_throws()
        {
            var tempBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var mapsDirectory = Path.Combine(tempBase, "source");
            var storageDirectory = Path.Combine(tempBase, "storage");
            var mapsFolder = Path.Combine(tempBase, "maps");
            Directory.CreateDirectory(mapsDirectory);
            Directory.CreateDirectory(storageDirectory);
            Directory.CreateDirectory(mapsFolder);

            try
            {
                const string mapName = "notatiff.tif";
                // A .tif file whose content is not a TIFF at all must be rejected.
                File.WriteAllText(Path.Combine(mapsDirectory, mapName), "this is not a tiff file");

                var mapStorage = new TestPlainStorage<MapBrowseItem>();
                var service = Create.Service.MapFileStorageService(
                    mapsStorage: mapStorage,
                    fileStorageConfig: Options.Create(new FileStorageConfig { TempData = mapsFolder }),
                    geospatialConfig: Options.Create(new GeospatialConfig())
                );

                // Act / Assert
                Assert.That(async () => await service.SaveOrUpdateMapAsync(BuildTiffMapFiles(mapName), mapsDirectory),
                    Throws.Exception.InstanceOf<InvalidOperationException>());
                Assert.That(mapStorage.GetById(mapName), Is.Null);
            }
            finally
            {
                Directory.Delete(tempBase, true);
            }
        }

        /// <summary>Writes a plain (non-georeferenced) 8×8 TIFF that carries no GeoTIFF tags.</summary>
        private static void WritePlainTiff(string path)
        {
            using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(8, 8);
            using var stream = File.Create(path);
            image.Save(stream, new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder());
        }

        private static MapFiles BuildTiffMapFiles(string mapName) => new MapFiles
        {
            Name = mapName,
            IsShapeFile = false,
            Size = 0,
            Files = new List<MapFile>
            {
                new MapFile { Name = mapName },
            }
        };

        /// <summary>
        /// Writes a minimal point shapefile whose "label" attribute contains the supplied values.
        /// Produces the .shp / .shx / .dbf triad that MapFileStorageService reads via Shapefile.OpenRead.
        /// </summary>
        private static void WriteShapefileWithLabels(string directory, string name, string[] labels)
        {
            var features = labels
                .Select((label, i) =>
                {
                    var point = new Point(new Coordinate(i, i));
                    var attrs = new AttributesTable { { "label", label } };
                    return (IFeature)new Feature(point, attrs);
                })
                .ToList();

            Shapefile.WriteAllFeatures(features, Path.Combine(directory, name + ".shp"));
        }

        private static MapFiles BuildShapeMapFiles(string mapName) => new MapFiles
        {
            Name = mapName,
            IsShapeFile = true,
            Size = 0,
            Files = new List<MapFile>
            {
                new MapFile { Name = mapName + ".shp" },
                new MapFile { Name = mapName + ".shx" },
                new MapFile { Name = mapName + ".dbf" },
            }
        };
    }
}
