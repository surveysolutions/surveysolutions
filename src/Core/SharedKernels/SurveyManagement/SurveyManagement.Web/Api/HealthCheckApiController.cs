using System;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [AllowAnonymous]
    public class HealthCheckApiController : ApiController
    {
        private readonly IUnhandledPackageStorage unhandledPackageStorage;
        private readonly IDatabaseHealthCheck databaseHealthCheck;
        private readonly IEventStoreHealthCheck eventStoreHealthCheck;
        private readonly IChunkReader chunkReader;
        private readonly IFolderPermissionChecker folderPermissionChecker;

        public HealthCheckApiController(IDatabaseHealthCheck databaseHealthCheck, 
            IEventStoreHealthCheck eventStoreHealthCheck, IUnhandledPackageStorage unhandledPackageStorage, 
            IChunkReader chunkReader, IFolderPermissionChecker folderPermissionChecker)
        {
            this.folderPermissionChecker = folderPermissionChecker;
            this.chunkReader = chunkReader;
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.databaseHealthCheck = databaseHealthCheck;
            this.unhandledPackageStorage = unhandledPackageStorage;
        }

        public HealthCheckStatus GetStatus()
        {
            var healthCheckStatus = GetHealthCheckModel();
            return healthCheckStatus.Status;
        }

        public HealthCheckModel GetVerboseStatus()
        {
            return GetHealthCheckModel();
        }

        private HealthCheckModel GetHealthCheckModel()
        {
            var databaseHealthCheckResult = databaseHealthCheck.Check();
            var eventStoreHealthCheckResult = eventStoreHealthCheck.Check();
            var numberOfUnhandledPackages = GetNumberOfUnhandledPackages();
            var numberOfSyncPackagesWithBigSize = GetNumberOfSyncPackagesWithBigSize();
            var folderPermissionCheckResult = folderPermissionChecker.Check();

            return new HealthCheckModel(databaseHealthCheckResult, eventStoreHealthCheckResult,
                numberOfUnhandledPackages, numberOfSyncPackagesWithBigSize,
                folderPermissionCheckResult);
        }

        private NumberHealthCheckResult GetNumberOfUnhandledPackages()
        {
            try
            {
                int count = unhandledPackageStorage.GetListOfUnhandledPackages().Count();
                return NumberHealthCheckResult.ForNumber(count);
            }
            catch (Exception e)
            {
                return NumberHealthCheckResult.Error("The information about unhandled packages can't be collected. " + e.Message);
            }
        }

        private NumberHealthCheckResult GetNumberOfSyncPackagesWithBigSize()
        {
            try
            {
                int count = chunkReader.GetNumberOfSyncPackagesWithBigSize();
                return NumberHealthCheckResult.ForNumber(count);
            }
            catch (Exception e)
            {
                return NumberHealthCheckResult.Error("The information about sync packages with big size can't be collected. " + e.Message);
            }
        }
    }
}