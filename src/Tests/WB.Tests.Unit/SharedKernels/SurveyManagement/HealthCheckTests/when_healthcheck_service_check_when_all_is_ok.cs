using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check_when_all_is_ok : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            var databaseHealthCheck = Mock.Of<IDatabaseHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var eventStoreHealthCheck = Mock.Of<IEventStoreHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var numberOfUnhandledPackagesChecker = Mock.Of<INumberOfUnhandledPackagesChecker>(m => m.Check() == NumberHealthCheckResult.Happy(0));
            var numberOfSyncPackagesWithBigSizeChecker = Mock.Of<INumberOfSyncPackagesWithBigSizeChecker>(m => m.Check() == NumberHealthCheckResult.Happy(0));
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null));

            service = CreateHealthCheckService(
                databaseHealthCheck,
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
