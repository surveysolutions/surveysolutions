using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.SyncPackageRestoreServiceTests
{
    internal class when_synchronization_item_doesnt_exist : SyncPackageRestoreServiceTestContext
    {
        Establish context = () =>
        {
            syncPackageRestoreService = CreateSyncPackageRestoreService();
        };
        
        Because of = () => result = syncPackageRestoreService.CheckAndApplySyncPackage(Guid.NewGuid());
        
        It should_result_be_true = () => result.ShouldBeTrue();

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static bool result;
    }
}
