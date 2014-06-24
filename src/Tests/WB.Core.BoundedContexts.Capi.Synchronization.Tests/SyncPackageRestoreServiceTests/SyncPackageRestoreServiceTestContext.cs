using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.SyncPackageRestoreServiceTests
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
                capiSynchronizationCacheService ?? Mock.Of<ICapiSynchronizationCacheService>(), stringCompressorMock.Object,
                jsonUtils ?? Mock.Of<IJsonUtils>(), commandService ?? Mock.Of<ICommandService>());
        }
    }
}
