using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.Synchronization.SyncStorage;
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
            var chunkReader = Mock.Of<IChunkReader>(m => m.GetNumberOfSyncPackagesWithBigSize() == 0);
            var folderPermissionChecker = Mock.Of<IFolderPermissionChecker>(m => m.Check() == new FolderPermissionCheckResult(null, null, null));

            controller = CreateHealthCheckApiController(
                databaseHealthCheck,
                eventStoreHealthCheck,
                brokenSyncPackagesStorage,
                chunkReader,
                folderPermissionChecker);
        };

        Because of = () =>
        {
            result = controller.GetStatus();
        };

        It should_return_HealthCheckStatus = () =>
            result.ShouldBeOfExactType<HealthCheckStatus>();

        It should_return_Happy_status = () =>
            result.ShouldEqual(HealthCheckStatus.Happy);


        private static HealthCheckStatus result;
        private static HealthCheckApiController controller;
        
    }
}
