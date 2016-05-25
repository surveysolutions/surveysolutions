﻿using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class when_performed_healthcheck_service_check : HealthCheckTestContext
    {
        private Establish context = () =>
        {
            eventStoreHealthCheckResult = EventStoreHealthCheckResult.Happy();
            numberOfUnhandledPackagesHealthCheckResult = NumberOfUnhandledPackagesHealthCheckResult.Happy(0);
            folderPermissionCheckResult = new FolderPermissionCheckResult(HealthCheckStatus.Happy, null, null, null);
            readSideHealthCheckResult =  ReadSideHealthCheckResult.Happy();

            eventStoreHealthCheckMock = new Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>>();
            eventStoreHealthCheckMock.Setup(m => m.Check()).Returns(eventStoreHealthCheckResult);
            numberOfUnhandledPackagesCheckerMock = new Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>();
            numberOfUnhandledPackagesCheckerMock.Setup(m => m.Check()).Returns(numberOfUnhandledPackagesHealthCheckResult);
            folderPermissionCheckerMock = new Mock<IAtomicHealthCheck<FolderPermissionCheckResult>>();
            folderPermissionCheckerMock.Setup(m => m.Check()).Returns(folderPermissionCheckResult);
            readSideHealthCheckResultMock = new Mock<IAtomicHealthCheck<ReadSideHealthCheckResult>>();
            readSideHealthCheckResultMock.Setup(m => m.Check()).Returns(readSideHealthCheckResult);

            service = CreateHealthCheckService(
                eventStoreHealthCheckMock.Object,
                numberOfUnhandledPackagesCheckerMock.Object,
                folderPermissionCheckerMock.Object,
                readSideHealthCheckResultMock.Object);
        };

        Because of = () =>
        {
            result = service.Check();
        };

        It should_return_HealthCheckModel = () =>
            result.ShouldBeOfExactType<HealthCheckResults>();

        It should_call_EventStoreHealthCheck_Check_once = () =>
            eventStoreHealthCheckMock.Verify(x => x.Check(), Times.Once());

        It should_return_EventStoreHealthCheckResult_after_call_EventStoreHealthCheck = () =>
            result.EventstoreConnectionStatus.ShouldEqual(eventStoreHealthCheckResult);

        It should_call_NumberOfUnhandledPackagesChecker_Check_once = () =>
            numberOfUnhandledPackagesCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_return_NumberOfUnhandledPackagesHealthCheckResult_after_call_NumberOfUnhandledPackagesChecker = () =>
           result.NumberOfUnhandledPackages.ShouldEqual(numberOfUnhandledPackagesHealthCheckResult);
        
        It should_call_FolderPermissionChecker_Check_once = () =>
            folderPermissionCheckerMock.Verify(x => x.Check(), Times.Once());

        It should_return_FolderPermissionCheckResult_after_call_FolderPermissionChecker = () =>
            result.FolderPermissionCheckResult.ShouldEqual(folderPermissionCheckResult);



        private static Mock<IAtomicHealthCheck<EventStoreHealthCheckResult>> eventStoreHealthCheckMock;
        private static Mock<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>> numberOfUnhandledPackagesCheckerMock;
        private static Mock<IAtomicHealthCheck<FolderPermissionCheckResult>> folderPermissionCheckerMock;
        private static Mock<IAtomicHealthCheck<ReadSideHealthCheckResult>> readSideHealthCheckResultMock;
        
        private static EventStoreHealthCheckResult eventStoreHealthCheckResult;
        private static NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackagesHealthCheckResult;
        private static FolderPermissionCheckResult folderPermissionCheckResult;
        private static ReadSideHealthCheckResult readSideHealthCheckResult;

        private static HealthCheckResults result;
        private static HealthCheckService service;
    }
}
