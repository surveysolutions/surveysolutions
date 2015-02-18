using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_there_are_any_errors : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            var databaseHealthCheck = Mock.Of<IDatabaseHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Down(databaseErrorMessage));
            var eventStoreHealthCheck = Mock.Of<IEventStoreHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Down(eventStoreErrorMessage));
            var brokenSyncPackagesStorage = Mock.Of<IBrokenSyncPackagesStorage>(m => m.GetListOfUnhandledPackages() == unhandledPackagesList);
        /*KP-4929     var chunkReader = Mock.Of<IChunkReader>(m => m.GetNumberOfSyncPackagesWithBigSize() == numberOfSyncPackagesWithBigSize);*/
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(currentUserName, allowedFoldersList, denidedFoldersList));

            service = CreateHealthCheckService(
                databaseHealthCheck,
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
           /*KP-4929      chunkReader,*/
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = service.Check();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

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

        It should_return_error_message_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.ErrorMessage.ShouldEqual(numberOfUnhandledPackagesErrorMessage);

        It should_return_4_packages_for_NumberOfUnhandledPackages_check = () =>
            result.NumberOfUnhandledPackages.Value.ShouldEqual(numberOfunhandledPackages);

      /*KP-4929   It should_return_Warning_status_for_NumberOfSyncPackagesWithBigSize_check = () =>
            result.NumberOfSyncPackagesWithBigSize.Status.ShouldEqual(HealthCheckStatus.Warning);

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

        private static HealthCheckResults result;
        private static HealthCheckService service;
        
    }
}
