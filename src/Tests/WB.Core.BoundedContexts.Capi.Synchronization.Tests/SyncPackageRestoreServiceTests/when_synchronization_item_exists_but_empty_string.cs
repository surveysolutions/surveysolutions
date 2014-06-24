using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.SyncPackageRestoreServiceTests
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

        Because of = () => result = syncPackageRestoreService.CheckAndApplySyncPackage(Guid.NewGuid());

        It should_result_be_false = () => result.ShouldBeFalse();

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static bool result;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
    }
}
