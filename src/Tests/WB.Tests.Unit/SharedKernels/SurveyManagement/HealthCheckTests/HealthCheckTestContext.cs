using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class HealthCheckTestContext
    {
        protected static HealthCheckService CreateHealthCheckService(
            IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck,
            IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker,
            IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult> numberOfSyncPackagesWithBigSizeChecker,
            IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker)
        {
            return new HealthCheckService(
                eventStoreHealthCheck ?? Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(),
                numberOfUnhandledPackagesChecker ?? Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(),
                numberOfSyncPackagesWithBigSizeChecker ?? Mock.Of<IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>>(),
                folderPermissionChecker ?? Mock.Of<IAtomicHealthCheck<FolderPermissionCheckResult>>());
        }
    }
}