using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_all_is_ok : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            var eventStoreHealthCheck = Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(m => m.Check() == EventStoreHealthCheckResult.Happy());
            var numberOfUnhandledPackagesChecker = Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(m => m.Check() == NumberOfUnhandledPackagesHealthCheckResult.Happy(0));
            var numberOfSyncPackagesWithBigSizeChecker = Mock.Of<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>>(m => m.Check() == NumberOfSyncPackagesWithBigSizeCheckResult.Happy(0));
            var folderPermissionChecker = Mock.Of<IAtomicHealthCheck<FolderPermissionCheckResult>>(m => m.Check() == new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null));

            service = CreateHealthCheckService(
                eventStoreHealthCheck,
                numberOfUnhandledPackagesChecker,
                numberOfSyncPackagesWithBigSizeChecker,
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = service.Check();
        };

        It should_return_HealthCheckResults = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

        It should_return_Happy_status = () =>
            result.Status.ShouldEqual(HealthCheckStatus.Happy);


        private static HealthCheckResults result;
        private static IHealthCheckService service;
        
    }
}
