using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.SyncPackageRestoreServiceTests
{
    internal class when_synchronization_item_doesnt_exist : SyncPackageRestoreServiceTestContext
    {
        Establish context = () =>
        {
            syncPackageRestoreService = CreateSyncPackageRestoreService();
        };
        
        Because of = () =>
             exception =
                    Catch.Exception(
                        () => syncPackageRestoreService.CheckAndApplySyncPackage(Guid.NewGuid()));

        It should_not_rise_any_exception = () => exception.ShouldBeNull();

        private static SyncPackageRestoreService syncPackageRestoreService;
        static Exception exception;
    }
}
