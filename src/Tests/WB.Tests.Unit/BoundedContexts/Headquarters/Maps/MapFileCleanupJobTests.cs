using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.PlainStorage;
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
            mapStorage.Setup(x => x.RemoveMapFileIfNotInUseAsync(fileName, fileName)).ThrowsAsync(new IOException());
            var job = new MapFileCleanupJob(
                requests,
                mapStorage.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Not.Null);
        }

        [Test]
        public async Task when_file_deletion_succeeds_should_remove_request()
        {
            const string fileName = "mapFile.tpk";
            var requests = new TestPlainStorage<MapFileDeletionRequest>();
            requests.Store(new MapFileDeletionRequest { Id = fileName, FileName = fileName }, fileName);
            var mapStorage = new Mock<IMapStorageService>();
            var job = new MapFileCleanupJob(
                requests,
                mapStorage.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Null);
            mapStorage.Verify(x => x.RemoveMapFileIfNotInUseAsync(fileName, fileName), Times.Once);
        }

        [Test]
        public async Task when_map_was_recreated_should_keep_its_file_and_remove_request()
        {
            const string fileName = "mapFile.tpk";
            var requests = new TestPlainStorage<MapFileDeletionRequest>();
            requests.Store(new MapFileDeletionRequest { Id = fileName, FileName = fileName }, fileName);
            var mapStorage = new Mock<IMapStorageService>();
            mapStorage.Setup(x => x.RemoveMapFileIfNotInUseAsync(fileName, fileName)).ReturnsAsync(false);
            var job = new MapFileCleanupJob(
                requests,
                mapStorage.Object,
                Mock.Of<ILogger<MapFileCleanupJob>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            Assert.That(requests.GetById(fileName), Is.Null);
            mapStorage.Verify(x => x.RemoveMapFileIfNotInUseAsync(fileName, fileName), Times.Once);
        }
    }
}
