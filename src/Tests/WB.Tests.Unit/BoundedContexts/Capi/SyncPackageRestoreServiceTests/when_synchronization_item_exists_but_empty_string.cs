using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.SyncPackageRestoreServiceTests
{
    internal class when_synchronization_item_exists_but_empty_string : SyncPackageRestoreServiceTestContext
    {
        Establish context = () =>
        {
            capiSynchronizationCacheServiceMock=new Mock<ICapiSynchronizationCacheService>();
            capiSynchronizationCacheServiceMock.Setup(x => x.DoesCachedItemExist(Moq.It.IsAny<Guid>())).Returns(true);
            capiSynchronizationCacheServiceMock.Setup(x => x.LoadItem(Moq.It.IsAny<Guid>())).Returns("");

            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock.Object);
        };

        Because of = () => syncPackageRestoreService.CheckAndApplySyncPackage(Guid.NewGuid());

        It should_call_LoadItem_once = () => capiSynchronizationCacheServiceMock.Verify(x => x.LoadItem(Moq.It.IsAny<Guid>()), Times.Once);

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
    }
}
