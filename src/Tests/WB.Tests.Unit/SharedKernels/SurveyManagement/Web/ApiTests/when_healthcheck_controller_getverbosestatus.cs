using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
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
        /*KP-4929     chunkReaderMock = new Mock<IChunkReader>();*/
            folderPermissionCheckerMock = new Mock<IFolderPermissionChecker>();

            controller = CreateHealthCheckApiController(
                databaseHealthCheckMock.Object,
                eventStoreHealthCheckMock.Object,
                brokenSyncPackagesStorageMock.Object,
              /*KP-4929   chunkReaderMock.Object,*/
                folderPermissionCheckerMock.Object);
        };

        Because of = () =>
        {
            result = controller.GetVerboseStatus();
        };

        It should_return_HealthCheckResults = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

        It should_return_Happy_status = () =>
            result.Status.ShouldEqual(checkResults.Status);

        It should_call_IHealthCheckService_Check_once = () =>
            serviceMock.Verify(x => x.Check(), Times.Once());


        It should_call_IBrokenSyncPackagesStorage_Check_once = () =>
            brokenSyncPackagesStorageMock.Verify(x => x.GetListOfUnhandledPackages(), Times.Once());

        /*KP-4929    It should_call_IChunkReader_Check_once = () =>
               chunkReaderMock.Verify(x => x.GetNumberOfSyncPackagesWithBigSize(), Times.Once());*/

           It should_call_IFolderPermissionChecker_Check_once = () =>
               folderPermissionCheckerMock.Verify(x => x.Check(), Times.Once());


           private static Mock<IDatabaseHealthCheck> databaseHealthCheckMock;
           private static Mock<IEventStoreHealthCheck> eventStoreHealthCheckMock;
           private static Mock<IBrokenSyncPackagesStorage> brokenSyncPackagesStorageMock;
   /*KP-4929        private static Mock<IChunkReader> chunkReaderMock;*/
        private static Mock<IFolderPermissionChecker> folderPermissionCheckerMock;

        private static HealthCheckModel result;
        private static HealthCheckApiController controller;
    }
}
