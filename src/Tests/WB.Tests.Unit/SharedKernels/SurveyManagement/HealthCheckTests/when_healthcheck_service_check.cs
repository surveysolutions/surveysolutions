using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_healthcheck_service_check : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            databaseHealthCheckMock = new Mock<IAtomicHealthCheck<RavenHealthCheckResult>>();
            databaseHealthCheckMock.Setup(m => m.Check()).Returns(RavenHealthCheckResult.Happy());
            eventStoreHealthCheckMock = new Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>>();
            eventStoreHealthCheckMock.Setup(m => m.Check()).Returns(EventStoreHealthCheckResult.Happy());
            numberOfUnhandledPackagesCheckerMock = new Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>();
            numberOfUnhandledPackagesCheckerMock.Setup(m => m.Check()).Returns(NumberOfUnhandledPackagesHealthCheckResult.Happy(0));
            numberOfSyncPackagesWithBigSizeCheckerMock = new Mock<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>>();
            numberOfSyncPackagesWithBigSizeCheckerMock.Setup(m => m.Check()).Returns(NumberOfSyncPackagesWithBigSizeCheckResult.Happy(0));
            folderPermissionCheckerMock = new Mock<IAtomicHealthCheck<FolderPermissionCheckResult>>();
            folderPermissionCheckerMock.Setup(m => m.Check()).Returns(new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null));

            service = CreateHealthCheckService(
                databaseHealthCheckMock.Object,
                eventStoreHealthCheckMock.Object,
                numberOfUnhandledPackagesCheckerMock.Object,
                numberOfSyncPackagesWithBigSizeCheckerMock.Object,
                folderPermissionCheckerMock.Object);
        };

        Because of = () =>
        {
            result = service.Check();
        };

        It should_return_HealthCheckModel = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

        It should_call_IDatabaseHealthCheck_Check_once = () =>
            databaseHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_call_IEventStoreHealthCheck_Check_once = () =>
            eventStoreHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_call_INumberOfUnhandledPackagesChecker_Check_once = () =>
            numberOfUnhandledPackagesCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_call_INumberOfSyncPackagesWithBigSizeChecker_Check_once = () =>
            numberOfSyncPackagesWithBigSizeCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_call_IFolderPermissionChecker_Check_once = () =>
            folderPermissionCheckerMock.Verify(x => x.Check(), Times.Once());


        private static Mock<IAtomicHealthCheck<RavenHealthCheckResult>> databaseHealthCheckMock;
        private static Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>> eventStoreHealthCheckMock;
        private static Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>> numberOfUnhandledPackagesCheckerMock;
        private static Mock<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>> numberOfSyncPackagesWithBigSizeCheckerMock;
        private static Mock<IAtomicHealthCheck<FolderPermissionCheckResult>> folderPermissionCheckerMock;

        private static HealthCheckResults result;
        private static HealthCheckService service;

    }
}
