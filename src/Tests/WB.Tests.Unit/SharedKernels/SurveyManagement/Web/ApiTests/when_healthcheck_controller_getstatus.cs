using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_healthcheck_controller_getstatus : ApiTestContext
    {
        private Establish context = () =>
        {
            var databaseHealthCheck = Mock.Of<IDatabaseHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var eventStoreHealthCheck = Mock.Of<IEventStoreHealthCheck>(m => m.Check() == ConnectionHealthCheckResult.Happy());
            var brokenSyncPackagesStorage = Mock.Of<IBrokenSyncPackagesStorage>(m => m.GetListOfUnhandledPackages() == Enumerable.Empty<string>());
         /*KP-4929    var chunkReader = Mock.Of<IChunkReader>(m => m.GetNumberOfSyncPackagesWithBigSize() == 0);*/
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(null, null, null));

            controller = CreateHealthCheckApiController(
                databaseHealthCheck,
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
          /*KP-4929       chunkReader,*/
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = controller.GetStatus();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckStatus>();

        It should_return_Happy_status = () =>
            result.ShouldEqual(checkResults.Status);

        It should_call_IHealthCheckService_Check_once = () =>
            serviceMock.Verify(x => x.Check(), Times.Once());


        private static HealthCheckResults checkResults;
        private static HealthCheckStatus result;
        private static Mock<IHealthCheckService> serviceMock;
        private static HealthCheckApiController controller;

    }
}
