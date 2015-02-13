using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.BoundedContexts.Capi.SyncPackageRestoreServiceTests
{
    internal class SyncPackageRestoreServiceTestContext
    {
        protected static SyncPackageRestoreService CreateSyncPackageRestoreService(
            ICapiSynchronizationCacheService capiSynchronizationCacheService = null,
            IJsonUtils jsonUtils = null, ICommandService commandService = null)
        {
            var stringCompressorMock = new Mock<IStringCompressor>();
            stringCompressorMock.Setup(x => x.DecompressString(Moq.It.IsAny<string>())).Returns<string>(s => s);
            return new SyncPackageRestoreService(Mock.Of<ILogger>(),
                capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(), commandService ?? Mock.Of<ICommandService>());
        }
    }
}
