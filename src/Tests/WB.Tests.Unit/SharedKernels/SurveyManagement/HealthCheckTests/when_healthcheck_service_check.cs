using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_performed_healthcheck_service_check : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            ravenHealthCheckResult = RavenHealthCheckResult.Happy();
            eventStoreHealthCheckResult = EventStoreHealthCheckResult.Happy();
            numberOfUnhandledPackagesHealthCheckResult = NumberOfUnhandledPackagesHealthCheckResult.Happy(0);
            numberOfSyncPackagesWithBigSizeCheckResult = NumberOfSyncPackagesWithBigSizeCheckResult.Happy(0);
            folderPermissionCheckResult = new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null);


            databaseHealthCheckMock = new Mock<IAtomicHealthCheck<RavenHealthCheckResult>>();
            databaseHealthCheckMock.Setup(m => m.Check()).Returns(ravenHealthCheckResult);
            eventStoreHealthCheckMock = new Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>>();
            eventStoreHealthCheckMock.Setup(m => m.Check()).Returns(eventStoreHealthCheckResult);
            numberOfUnhandledPackagesCheckerMock = new Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>();
            numberOfUnhandledPackagesCheckerMock.Setup(m => m.Check()).Returns(numberOfUnhandledPackagesHealthCheckResult);
            numberOfSyncPackagesWithBigSizeCheckerMock = new Mock<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>>();
            numberOfSyncPackagesWithBigSizeCheckerMock.Setup(m => m.Check()).Returns(numberOfSyncPackagesWithBigSizeCheckResult);
            folderPermissionCheckerMock = new Mock<IAtomicHealthCheck<FolderPermissionCheckResult>>();
            folderPermissionCheckerMock.Setup(m => m.Check()).Returns(folderPermissionCheckResult);

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

        It should_call_DatabaseHealthCheck_Check_once = () =>
            databaseHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_return_RavenHealthCheckResult_after_call_RavenHealthCheck = () =>
            result.DatabaseConnectionStatus.ShouldEqual(ravenHealthCheckResult);

        It should_call_EventStoreHealthCheck_Check_once = () =>
            eventStoreHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_return_EventStoreHealthCheckResult_after_call_EventStoreHealthCheck = () =>
            result.EventstoreConnectionStatus.ShouldEqual(eventStoreHealthCheckResult);

        It should_call_NumberOfUnhandledPackagesChecker_Check_once = () =>
            numberOfUnhandledPackagesCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_return_NumberOfUnhandledPackagesHealthCheckResult_after_call_NumberOfUnhandledPackagesChecker = () =>
           result.NumberOfUnhandledPackages.ShouldEqual(numberOfUnhandledPackagesHealthCheckResult);
        
        It should_call_NumberOfSyncPackagesWithBigSizeChecker_Check_once = () =>
            numberOfSyncPackagesWithBigSizeCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_return_NumberOfSyncPackagesWithBigSizeHealthCheckResult_after_call_NumberOfSyncPackagesWithBigSizeChecker = () =>
            result.NumberOfSyncPackagesWithBigSize.ShouldEqual(numberOfSyncPackagesWithBigSizeCheckResult);

        It should_call_FolderPermissionChecker_Check_once = () =>
            folderPermissionCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_return_FolderPermissionCheckResult_after_call_FolderPermissionChecker = () =>
            result.FolderPermissionCheckResult.ShouldEqual(folderPermissionCheckResult);



        private static Mock<IAtomicHealthCheck<RavenHealthCheckResult>> databaseHealthCheckMock;
        private static Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>> eventStoreHealthCheckMock;
        private static Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>> numberOfUnhandledPackagesCheckerMock;
        private static Mock<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>> numberOfSyncPackagesWithBigSizeCheckerMock;
        private static Mock<IAtomicHealthCheck<FolderPermissionCheckResult>> folderPermissionCheckerMock;

        private static RavenHealthCheckResult ravenHealthCheckResult;
        private static EventStoreHealthCheckResult eventStoreHealthCheckResult;
        private static NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackagesHealthCheckResult;
        private static NumberOfSyncPackagesWithBigSizeCheckResult numberOfSyncPackagesWithBigSizeCheckResult;
        private static FolderPermissionCheckResult folderPermissionCheckResult;


        private static HealthCheckResults result;
        private static HealthCheckService service;

    }
}
