using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_there_are_any_warnings : HealthCheckTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var eventStoreHealthCheck = Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(m => m.Check() == EventStoreHealthCheckResult.Happy());
            var numberOfUnhandledPackagesChecker = Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(m => m.Check() == NumberOfUnhandledPackagesHealthCheckResult.Warning(numberOfunhandledPackages, numberOfUnhandledPackagesErrorMessage));
            var folderPermissionChecker = Mock.Of<IAtomicHealthCheck<FolderPermissionCheckResult>>(m => m.Check() == new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null));
            var readSideHealthChecker = Mock.Of<IAtomicHealthCheck<ReadSideHealthCheckResult>>(m => m.Check() == ReadSideHealthCheckResult.Happy());

            service = CreateHealthCheckService(
                eventStoreHealthCheck,
                numberOfUnhandledPackagesChecker,
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
            result.Status.Should().Be(HealthCheckStatus.Warning);

        [NUnit.Framework.Test] public void should_return_Down_status_for_EventStore_check () =>
            result.EventstoreConnectionStatus.Status.Should().Be(HealthCheckStatus.Happy);

        [NUnit.Framework.Test] public void should_return_empty_error_message_for_EventStore_check () =>
            result.EventstoreConnectionStatus.ErrorMessage.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_Warning_status_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.Status.Should().Be(HealthCheckStatus.Warning);

        [NUnit.Framework.Test] public void should_return_error_message_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.ErrorMessage.Should().Be(numberOfUnhandledPackagesErrorMessage);

        [NUnit.Framework.Test] public void should_return_4_packages_for_NumberOfUnhandledPackages_check () =>
            result.NumberOfUnhandledPackages.Value.Should().Be(numberOfunhandledPackages);

        [NUnit.Framework.Test] public void should_return_Down_status_for_FolderPermissionCheckResult_check () =>
            result.FolderPermissionCheckResult.Status.Should().Be(HealthCheckStatus.Happy);


        private static string numberOfUnhandledPackagesErrorMessage = "numberOfUnhandledPackagesErrorMessage error message";
        private static int numberOfunhandledPackages = 4;

        private static HealthCheckResults result;
        private static HealthCheckService service;
        
    }
}
