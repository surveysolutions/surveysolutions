using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_healthcheck_controller_getverbosestatus : ApiTestContext
    {
        private Establish context = () =>
        {
            databaseHealthCheckMock = new Mock<IDatabaseHealthCheck>();
            eventStoreHealthCheckMock = new Mock<IEventStoreHealthCheck>();
            brokenSyncPackagesStorageMock = new Mock<IBrokenSyncPackagesStorage>();
            chunkReaderMock = new Mock<IChunkReader>();
            folderPermissionCheckerMock = new Mock<IFolderPermissionChecker>();

            controller = CreateHealthCheckApiController(
                databaseHealthCheckMock.Object,
                eventStoreHealthCheckMock.Object,
                brokenSyncPackagesStorageMock.Object,
                chunkReaderMock.Object,
                folderPermissionCheckerMock.Object);
        };

        Because of = () =>
        {
            result = controller.GetVerboseStatus();
        };

        It should_return_HealthCheckModel = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

        It should_call_IDatabaseHealthCheck_Check_once = () =>
            databaseHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_call_IEventStoreHealthCheck_Check_once = () =>
            eventStoreHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_call_IBrokenSyncPackagesStorage_Check_once = () =>
            brokenSyncPackagesStorageMock.Verify(x => x.GetListOfUnhandledPackages(), Times.Once());

        It should_call_IChunkReader_Check_once = () =>
            chunkReaderMock.Verify(x => x.GetNumberOfSyncPackagesWithBigSize(), Times.Once());

        It should_call_IFolderPermissionChecker_Check_once = () =>
            folderPermissionCheckerMock.Verify(x => x.Check(), Times.Once());


        private static Mock<IDatabaseHealthCheck> databaseHealthCheckMock;
        private static Mock<IEventStoreHealthCheck> eventStoreHealthCheckMock;
        private static Mock<IBrokenSyncPackagesStorage> brokenSyncPackagesStorageMock;
        private static Mock<IChunkReader> chunkReaderMock;
        private static Mock<IFolderPermissionChecker> folderPermissionCheckerMock;

        private static HealthCheckResults result;
        private static HealthCheckApiController controller;

    }
}
