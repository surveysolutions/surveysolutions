using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_healthcheck_controller_getverbosestatus_when_there_are_any_warnings : ApiTestContext
    {
        private Establish context = () =>
        {
            var databaseHealthCheck = Mock.Of<IDatabaseHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var eventStoreHealthCheck = Mock.Of<IEventStoreHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var brokenSyncPackagesStorage = Mock.Of<IBrokenSyncPackagesStorage>(m => m.GetListOfUnhandledPackages() == unhandledPackagesList);
          /*KP-4929   var chunkReader = Mock.Of<IChunkReader>(m => m.GetNumberOfSyncPackagesWithBigSize() == numberOfSyncPackagesWithBigSize);*/
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(null, null, null));

            controller = CreateHealthCheckApiController(
                databaseHealthCheck,
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
            /*KP-4929     chunkReader,*/
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = controller.GetVerboseStatus();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckModel>();

        It should_return_Down_status = () =>
            result.Status.ShouldEqual(HealthCheckStatus.Warning);

        It should_return_Down_status_for_database_check = () =>
            result.DatabaseConnectionStatus.Status.ShouldEqual(HealthCheckStatus.Happy);

        It should_return_empty_error_message_for_database_check = () =>
            result.DatabaseConnectionStatus.ErrorMessage.ShouldBeNull();

        It should_return_Down_status_for_EventStore_check = () =>
            result.EventstoreConnectionStatus.Status.ShouldEqual(HealthCheckStatus.Happy);

        It should_return_empty_error_message_for_EventStore_check = () =>
            result.EventstoreConnectionStatus.ErrorMessage.ShouldBeNull();

        It should_return_Warning_status_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.Status.ShouldEqual(HealthCheckStatus.Warning);

        It should_return_1_package_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.Value.ShouldEqual(unhandledPackagesList.Length);

        It should_return_error_message_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.ErrorMessage.ShouldNotBeEmpty();

  /*KP-4929       It should_return_Warning_status_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.Status.ShouldEqual(HealthCheckStatus.Warning);

        It should_return_5_packages_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.Value.ShouldEqual(numberOfSyncPackagesWithBigSize);

        It should_return_error_message_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.ErrorMessage.ShouldNotBeEmpty();*/

        It should_return_Down_status_for_FolderPermissionCheckResult_check = () =>
            result.FolderPermissionCheckResult.Status.ShouldEqual(HealthCheckStatus.Happy);


   /*KP-4929      private static int      numberOfSyncPackagesWithBigSize = 5;*/
        private static string[] unhandledPackagesList = new[] { "package name" };

        private static HealthCheckModel result;
        private static HealthCheckApiController controller;
        
    }
}
