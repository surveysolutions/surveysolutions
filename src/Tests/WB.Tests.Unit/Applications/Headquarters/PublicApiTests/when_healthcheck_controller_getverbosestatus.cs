using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_healthcheck_controller_getverbosestatus : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            /*KP-4929    databaseHealthCheckMock = new Mock<IDatabaseHealthCheck>();
                eventStoreHealthCheckMock = new Mock<IEventStoreHealthCheck>();
                brokenSyncPackagesStorageMock = new Mock<IBrokenSyncPackagesStorage>();
                 chunkReaderMock = new Mock<IChunkReader>();
            folderPermissionCheckerMock = new Mock<IFolderPermissionChecker>();*/
            healthCheckService = new Mock<IHealthCheckService>();
            healthCheckService.Setup(x => x.Check())
                .Returns(new HealthCheckResults(HealthCheckStatus.Happy, 
                    EventStoreHealthCheckResult.Happy(), NumberOfUnhandledPackagesHealthCheckResult.Happy(0),
                    new FolderPermissionCheckResult(HealthCheckStatus.Happy, "t", new string[0], new string[0]), ReadSideHealthCheckResult.Happy()));
            controller = CreateHealthCheckApiController(healthCheckService.Object
                /*KP-4929  databaseHealthCheckMock.Object,
                  eventStoreHealthCheckMock.Object,
                  brokenSyncPackagesStorageMock.Object,
                   chunkReaderMock.Object,
                folderPermissionCheckerMock.Object*/);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            result = controller.GetVerboseStatus();
        }

        [NUnit.Framework.Test] public void should_return_HealthCheckResults () =>
            result.Should().BeOfType<HealthCheckResults>();

        [NUnit.Framework.Test] public void should_return_Happy_status () =>
            result.Status.Should().Be(HealthCheckStatus.Happy);

        /*KP-4929    [NUnit.Framework.Test] public void should_call_IHealthCheckService_Check_once () =>
             serviceMock.Verify(x => x.Check(), Times.Once());


         [NUnit.Framework.Test] public void should_call_IBrokenSyncPackagesStorage_Check_once () =>
             brokenSyncPackagesStorageMock.Verify(x => x.GetListOfUnhandledPackages(), Times.Once());

          [NUnit.Framework.Test] public void should_call_IChunkReader_Check_once () =>
                chunkReaderMock.Verify(x => x.GetNumberOfSyncPackagesWithBigSize(), Times.Once());*/

           [NUnit.Framework.Test] public void should_call_IFolderPermissionChecker_Check_once () =>
               healthCheckService.Verify(x => x.Check(), Times.Once());


           /*KP-4929            private static Mock<IDatabaseHealthCheck> databaseHealthCheckMock;
                  private static Mock<IEventStoreHealthCheck> eventStoreHealthCheckMock;
                  private static Mock<IBrokenSyncPackagesStorage> brokenSyncPackagesStorageMock;
             private static Mock<IChunkReader> chunkReaderMock;
        private static Mock<IFolderPermissionChecker> folderPermissionCheckerMock;
        */
        private static Mock<IHealthCheckService> healthCheckService;
        private static HealthCheckResults result;
        private static HealthCheckApiController controller;
    }
}
