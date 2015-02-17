using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.HealthCheckTests
{
    internal class HealthCheckTestContext
    {
        protected static HealthCheckService CreateHealthCheckService(
            IDatabaseHealthCheck databaseHealthCheck,
            IEventStoreHealthCheck eventStoreHealthCheck,
            INumberOfUnhandledPackagesChecker numberOfUnhandledPackagesChecker,
            INumberOfSyncPackagesWithBigSizeChecker numberOfSyncPackagesWithBigSizeChecker, 
            IFolderPermissionChecker folderPermissionChecker)
        {
            return new HealthCheckService(
                databaseHealthCheck ?? Mock.Of<IDatabaseHealthCheck>(),
                eventStoreHealthCheck ?? Mock.Of<IEventStoreHealthCheck>(),
                numberOfUnhandledPackagesChecker ?? Mock.Of<INumberOfUnhandledPackagesChecker>(),
                numberOfSyncPackagesWithBigSizeChecker ?? Mock.Of<INumberOfSyncPackagesWithBigSizeChecker>(),
                folderPermissionChecker ?? Mock.Of<IFolderPermissionChecker>());
        }
    }
}