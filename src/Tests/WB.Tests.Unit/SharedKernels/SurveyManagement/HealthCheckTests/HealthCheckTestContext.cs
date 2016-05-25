using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class HealthCheckTestContext
    {
        protected static HealthCheckService CreateHealthCheckService(
            IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck,
            IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker,
            IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker,
            IAtomicHealthCheck<ReadSideHealthCheckResult> readSideHealthCheckResult)
        {
            return new HealthCheckService(
                eventStoreHealthCheck ?? Mock.Of<IAtomicHealthCheck<EventStoreHealthCheckResult>>(),
                numberOfUnhandledPackagesChecker ?? Mock.Of<IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>>(),
                folderPermissionChecker ?? Mock.Of<IAtomicHealthCheck<FolderPermissionCheckResult>>(),
                readSideHealthCheckResult ?? Mock.Of<IAtomicHealthCheck<ReadSideHealthCheckResult>>());
        }
    }
}