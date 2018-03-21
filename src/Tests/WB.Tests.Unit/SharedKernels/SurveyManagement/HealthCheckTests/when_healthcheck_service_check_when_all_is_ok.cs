using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_all_is_ok : HealthCheckTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var eventStoreHealthCheck = Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(m => m.Check() == EventStoreHealthCheckResult.Happy());
            var numberOfUnhandledPackagesChecker = Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(m => m.Check() == NumberOfUnhandledPackagesHealthCheckResult.Happy(0));
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

        [NUnit.Framework.Test] public void should_return_HealthCheckResults () =>
            result.Should().BeOfType<HealthCheckResults>();

        [NUnit.Framework.Test] public void should_return_Happy_status () =>
            result.Status.Should().Be(HealthCheckStatus.Happy);


        private static HealthCheckResults result;
        private static IHealthCheckService service;
        
    }
}
