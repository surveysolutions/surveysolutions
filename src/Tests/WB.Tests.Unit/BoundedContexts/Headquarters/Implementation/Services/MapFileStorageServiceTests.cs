using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.SharedKernels.Configs;
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
