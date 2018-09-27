using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.MapSynchronizerTests
{
    public class MapSynchronizerTests
    {
        [Test]
        public async Task should_always_delete_map_file()
        {
            var synchronizationService = new Mock<IOnlineSynchronizationService>();
            synchronizationService.Setup(x => x.GetMapList(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MapView>()
                {
                    new MapView(){MapName = "test"}
                });

            var mapService = new Mock<IMapService>();
            mapService.Setup(x => x.GetAvailableMaps()).Returns(new List<MapDescription>()
            {
                new MapDescription(){MapName = "test"},
                new MapDescription(){MapName = "test1"}
            });

            mapService.Setup(x => x.DoesMapExist(It.IsAny<string>())).Returns(true);

            

            var service = Create.Service.MapSyncProvider(
                synchronizationService : synchronizationService.Object,
                mapService : mapService.Object);
            
            await service.Synchronize(
                new Progress<SyncProgressInfo>(),
                new CancellationToken(),  
                new SynchronizationStatistics());
            
            // assert
            mapService.Verify(x => x.RemoveMap("test1"), Times.Once);
            
            //Assert.That(token, Is.EqualTo("offline sync token"));
        }
    }
}
