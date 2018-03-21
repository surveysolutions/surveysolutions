using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_there_are_any_errors : HealthCheckTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var eventStoreHealthCheck = Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(m => m.Check() == EventStoreHealthCheckResult.Down(eventStoreErrorMessage));
            var brokenSyncPackagesStorage = Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(m => m.Check() == NumberOfUnhandledPackagesHealthCheckResult.Warning(numberOfunhandledPackages,numberOfUnhandledPackagesErrorMessage));
            var folderPermissionChecker = Mock.Of<IAtomicHealthCheck<FolderPermissionCheckResult>>(m => m.Check() == new FolderPermissionCheckResult(HealthCheckStatus.Down, currentUserName, allowedFoldersList, denidedFoldersList));
            var readSideHealthChecker = Mock.Of<IAtomicHealthCheck<ReadSideHealthCheckResult>>(m => m.Check() == ReadSideHealthCheckResult.Happy());

            service = CreateHealthCheckService(
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
                folderPermissionChecker,
                readSideHealthChecker);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            result = service.Check();
        }

        [NUnit.Framework.Test] public void should_return_HealthCheckStatus () =>
            result.Should().BeOfType<HealthCheckResults>();

        [NUnit.Framework.Test] public void should_return_Down_status () =>
            result.Status.Should().Be(HealthCheckStatus.Down);

        [NUnit.Framework.Test] public void should_return_Down_status_for_EventStore_check () =>
            result.EventstoreConnectionStatus.Status.Should().Be(HealthCheckStatus.Down);

        [NUnit.Framework.Test] public void should_return_error_message_for_EventStore_check () =>
            result.EventstoreConnectionStatus.ErrorMessage.Should().Be(eventStoreErrorMessage);

        [NUnit.Framework.Test] public void should_return_Warning_status_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.Status.Should().Be(HealthCheckStatus.Warning);

        [NUnit.Framework.Test] public void should_return_error_message_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.ErrorMessage.Should().Be(numberOfUnhandledPackagesErrorMessage);

        [NUnit.Framework.Test] public void should_return_4_packages_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.Value.Should().Be(numberOfunhandledPackages);

        [NUnit.Framework.Test] public void should_return_Down_status_for_FolderPermissionCheckResult_check () =>
            result.FolderPermissionCheckResult.Status.Should().Be(HealthCheckStatus.Down);

        [NUnit.Framework.Test] public void should_return_user_name_for_FolderPermissionCheckResult_check () =>
            result.FolderPermissionCheckResult.ProcessRunedUnder.Should().Be(currentUserName);

        [NUnit.Framework.Test] public void should_return_allowed_folders_for_FolderPermissionCheckResult_check () =>
            result.FolderPermissionCheckResult.AllowedFolders.Should().BeEquivalentTo(allowedFoldersList);

        [NUnit.Framework.Test] public void should_return_denided_folders_for_FolderPermissionCheckResult_check () =>
            result.FolderPermissionCheckResult.DeniedFolders.Should().BeEquivalentTo(denidedFoldersList);


        private static string   eventStoreErrorMessage = "eventStore error message";
        private static int numberOfunhandledPackages = 3;
        private static string numberOfUnhandledPackagesErrorMessage = "numberOfUnhandledPackagesErrorMessage error message";
        private static string   currentUserName = "user name";
        private static string[] allowedFoldersList = new[] { "allow folder" };
        private static string[] denidedFoldersList = new[] { "deny folder" };

        private static HealthCheckResults result;
        private static HealthCheckService service;
        
    }
}
