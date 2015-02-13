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
    internal class when_healthcheck_controller_getverbosestatus_when_there_are_any_errors : ApiTestContext
    {
        private Establish context = () =>
        {
            var databaseHealthCheck = Mock.Of<IDatabaseHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Down(databaseErrorMessage));
            var eventStoreHealthCheck = Mock.Of<IEventStoreHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Down(eventStoreErrorMessage));
            var brokenSyncPackagesStorage = Mock.Of<IBrokenSyncPackagesStorage>(m => m.GetListOfUnhandledPackages() == unhandledPackagesList);
        /*KP-4929     var chunkReader = Mock.Of<IChunkReader>(m => m.GetNumberOfSyncPackagesWithBigSize() == numberOfSyncPackagesWithBigSize);*/
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(currentUserName, allowedFoldersList, denidedFoldersList));

            controller = CreateHealthCheckApiController(
                databaseHealthCheck,
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
           /*KP-4929      chunkReader,*/
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = controller.GetVerboseStatus();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckModel>();

        It should_return_Down_status = () =>
            result.Status.ShouldEqual(HealthCheckStatus.Down);

        It should_return_Down_status_for_database_check = () =>
            result.DatabaseConnectionStatus.Status.ShouldEqual(HealthCheckStatus.Down);

        It should_return_error_message_for_database_check = () =>
            result.DatabaseConnectionStatus.ErrorMessage.ShouldEqual(databaseErrorMessage);

        It should_return_Down_status_for_EventStore_check = () =>
            result.EventstoreConnectionStatus.Status.ShouldEqual(HealthCheckStatus.Down);

        It should_return_error_message_for_EventStore_check = () =>
            result.EventstoreConnectionStatus.ErrorMessage.ShouldEqual(eventStoreErrorMessage);

        It should_return_Warning_status_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.Status.ShouldEqual(HealthCheckStatus.Warning);

        It should_return_1_package_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.Value.ShouldEqual(unhandledPackagesList.Length);

        It should_return_error_message_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.ErrorMessage.ShouldNotBeEmpty();

      /*KP-4929   It should_return_Warning_status_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.Status.ShouldEqual(HealthCheckStatus.Warning);

        It should_return_5_packages_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.Value.ShouldEqual(numberOfSyncPackagesWithBigSize);

        It should_return_error_message_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.ErrorMessage.ShouldNotBeEmpty();*/

        It should_return_Down_status_for_FolderPermissionCheckResult_check = () =>
            result.FolderPermissionCheckResult.Status.ShouldEqual(HealthCheckStatus.Down);

        It should_return_user_name_for_FolderPermissionCheckResult_check = () =>
            result.FolderPermissionCheckResult.ProcessRunedUnder.ShouldEqual(currentUserName);

        It should_return_allowed_folders_for_FolderPermissionCheckResult_check = () =>
            result.FolderPermissionCheckResult.AllowedFolders.ShouldEqual(allowedFoldersList);

        It should_return_denided_folders_for_FolderPermissionCheckResult_check = () =>
            result.FolderPermissionCheckResult.DenidedFolders.ShouldEqual(denidedFoldersList);


        private static string   databaseErrorMessage = "database error message";
        private static string   eventStoreErrorMessage = "eventStore error message";
    /*KP-4929     private static int      numberOfSyncPackagesWithBigSize = 5;*/
        private static string[] unhandledPackagesList = new[] { "package name" };
        private static string   currentUserName = "user name";
        private static string[] allowedFoldersList = new[] { "allow folder" };
        private static string[] denidedFoldersList = new[] { "deny folder" };

        private static HealthCheckModel result;
        private static HealthCheckApiController controller;
        
    }
}
