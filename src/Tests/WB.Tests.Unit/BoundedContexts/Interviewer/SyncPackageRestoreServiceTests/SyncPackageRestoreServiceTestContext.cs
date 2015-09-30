using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.SyncPackageRestoreServiceTests
{
    internal class SyncPackageRestoreServiceTestContext
    {
        protected static SyncPackageRestoreService CreateSyncPackageRestoreService(
            ICapiSynchronizationCacheService capiSynchronizationCacheService = null,
            ISerializer serializer = null, ICommandService commandService = null)
        {
            var stringCompressorMock = new Mock<IStringCompressor>();
            stringCompressorMock.Setup(x => x.DecompressString(Moq.It.IsAny<string>())).Returns<string>(s => s);
            return new SyncPackageRestoreService(Mock.Of<ILogger>(),
                capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(),
                serializer ?? Mock.Of<ISerializer>(), commandService ?? Mock.Of<ICommandService>());
        }
    }
}
