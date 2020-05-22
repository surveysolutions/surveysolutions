using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
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
    }
}
