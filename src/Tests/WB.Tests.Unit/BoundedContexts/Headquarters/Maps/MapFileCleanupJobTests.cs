using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Maps
{
    [TestFixture]
    [TestOf(typeof(MapFileCleanupJob))]
    public class MapFileCleanupJobTests
    {
        [Test]
        public async Task when_file_deletion_fails_should_keep_request_for_retry()
        {
            const string fileName = "mapFile.tpk";
            var requests = new TestPlainStorage<MapFileDeletionRequest>();
            requests.Store(new MapFileDeletionRequest { Id = fileName, FileName = fileName }, fileName);
            var mapStorage = new Mock<IMapStorageService>();
            mapStorage.Setup(x => x.RemoveMapFileAsync(fileName)).ThrowsAsync(new IOException());
            var maps = new TestPlainStorage<MapBrowseItem>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var job = new MapFileCleanupJob(
                requests,
                maps,
                mapStorage.Object,
                unitOfWork.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Not.Null);
            unitOfWork.Verify(x => x.Complete(), Times.Never);
        }

        [Test]
        public async Task when_file_deletion_succeeds_should_remove_request()
        {
            const string fileName = "mapFile.tpk";
            var requests = new TestPlainStorage<MapFileDeletionRequest>();
            requests.Store(new MapFileDeletionRequest { Id = fileName, FileName = fileName }, fileName);
            var maps = new TestPlainStorage<MapBrowseItem>();
            var mapStorage = new Mock<IMapStorageService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var job = new MapFileCleanupJob(
                requests,
                maps,
                mapStorage.Object,
                unitOfWork.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Null);
            unitOfWork.Verify(x => x.AcceptChanges(), Times.Once);
            unitOfWork.Verify(x => x.Complete(), Times.Once);
        }

        [Test]
        public async Task when_map_was_recreated_should_keep_its_file_and_remove_request()
        {
            const string fileName = "mapFile.tpk";
            var requests = new TestPlainStorage<MapFileDeletionRequest>();
            requests.Store(new MapFileDeletionRequest { Id = fileName, FileName = fileName }, fileName);
            var maps = new TestPlainStorage<MapBrowseItem>();
            maps.Store(Create.Entity.MapBrowseItem(fileName), fileName);
            var mapStorage = new Mock<IMapStorageService>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var job = new MapFileCleanupJob(
                requests,
                maps,
                mapStorage.Object,
                unitOfWork.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Null);
            mapStorage.Verify(x => x.RemoveMapFileAsync(It.IsAny<string>()), Times.Never);
            unitOfWork.Verify(x => x.Complete(), Times.Once);
        }
    }
}
