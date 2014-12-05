using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Capi.SyncPackageRestoreServiceTests
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
