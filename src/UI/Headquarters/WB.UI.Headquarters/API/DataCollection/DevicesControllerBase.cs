using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class DevicesControllerBase : ApiController
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly HqUserManager userManager;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository;

        protected DevicesControllerBase(
            IAuthorizedUser authorizedUser,
            ISyncProtocolVersionProvider syncVersionProvider,
            HqUserManager userManager,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository)
        {
            this.authorizedUser = authorizedUser;
            this.syncVersionProvider = syncVersionProvider;
            this.userManager = userManager;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.syncLogRepository = syncLogRepository;
        }
        
        public virtual HttpResponseMessage CanSynchronize(string id, int version)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();
            int supervisorShiftVersionNumber = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (version != supervisorRevisionNumber || version < supervisorShiftVersionNumber)
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }
            
            return this.authorizedUser.DeviceId != id
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK);
        }
        
        public virtual HttpResponseMessage LinkCurrentResponsibleToDevice(string id, int version)
        {
            this.userManager.LinkDeviceToInterviewerOrSupervisor(this.authorizedUser.Id, id, DateTime.UtcNow);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        public virtual async Task<IHttpActionResult> Info(DeviceInfoApiView info)
        {
            var deviceLocation = info.DeviceLocation;

            var user = await this.userManager.FindByIdAsync(this.authorizedUser.Id);

            if (user == null) return this.Unauthorized();

            this.deviceSyncInfoRepository.AddOrUpdate(new DeviceSyncInfo
            {
                SyncDate = DateTime.UtcNow,
                InterviewerId = this.authorizedUser.Id,
                DeviceId = info.DeviceId,
                LastAppUpdatedDate = info.LastAppUpdatedDate,
                DeviceModel = info.DeviceModel,
                DeviceType = info.DeviceType,
                AndroidVersion = info.AndroidVersion,
                DeviceLanguage = info.DeviceLanguage,
                DeviceBuildNumber = info.DeviceBuildNumber,
                DeviceSerialNumber = info.DeviceSerialNumber,
                DeviceManufacturer = info.DeviceManufacturer,
                DBSizeInfo = info.DBSizeInfo,
                AndroidSdkVersion = info.AndroidSdkVersion,
                AndroidSdkVersionName = info.AndroidSdkVersionName,
                DeviceDate = info.DeviceDate,
                AppVersion = info.AppVersion,
                AppBuildVersion = info.AppBuildVersion,
                MobileSignalStrength = info.MobileSignalStrength,
                AppOrientation = info.AppOrientation,
                MobileOperator = info.MobileOperator,
                NetworkSubType = info.NetworkSubType,
                NetworkType = info.NetworkType,
                BatteryChargePercent = info.BatteryChargePercent,
                BatteryPowerSource = info.BatteryPowerSource,
                IsPowerInSaveMode = info.IsPowerInSaveMode,
                DeviceLocationLat = deviceLocation?.Latitude,
                DeviceLocationLong = deviceLocation?.Longitude,
                NumberOfStartedInterviews = info.NumberOfStartedInterviews,
                RAMFreeInBytes = info.RAMInfo?.Free ?? 0,
                RAMTotalInBytes = info.RAMInfo?.Total ?? 0,
                StorageFreeInBytes = info.StorageInfo?.Free ?? 0,
                StorageTotalInBytes = info.StorageInfo?.Total ?? 0
            });

            user.Profile.DeviceAppBuildVersion = info.AppBuildVersion;
            user.Profile.DeviceAppVersion = info.AppVersion;

            await this.userManager.UpdateAsync(user);

            return this.Ok();
        }

        public virtual IHttpActionResult Statistics(SyncStatisticsApiView statistics)
        {
            var deviceInfo = this.deviceSyncInfoRepository.GetLastByInterviewerId(this.authorizedUser.Id);
            deviceInfo.Statistics = new SyncStatistics
            {
                DownloadedInterviewsCount = statistics.DownloadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.DownloadedQuestionnairesCount,
                UploadedInterviewsCount = statistics.UploadedInterviewsCount,
                NewInterviewsOnDeviceCount = statistics.NewInterviewsOnDeviceCount,
                RejectedInterviewsOnDeviceCount = statistics.RejectedInterviewsOnDeviceCount,
                RemovedAssignmentsCount = statistics.RemovedAssignmentsCount,
                RemovedInterviewsCount = statistics.RemovedInterviewsCount,
                SyncFinishDate = DateTime.UtcNow,
                TotalConnectionSpeed = statistics.TotalConnectionSpeed,
                TotalDownloadedBytes = statistics.TotalDownloadedBytes,
                TotalUploadedBytes = statistics.TotalUploadedBytes,
                TotalSyncDuration = statistics.TotalSyncDuration,

                AssignmentsOnDeviceCount = statistics.AssignmentsOnDeviceCount,
                NewAssignmentsCount = statistics.NewAssignmentsCount
            };
            this.deviceSyncInfoRepository.AddOrUpdate(deviceInfo);

            return this.Ok();
        }

        public virtual IHttpActionResult UnexpectedException(UnexpectedExceptionApiView exception)
        {
            this.syncLogRepository.Store(new SynchronizationLogItem
            {
                DeviceId = this.authorizedUser.DeviceId,
                InterviewerId = this.authorizedUser.Id,
                InterviewerName = this.authorizedUser.UserName,
                LogDate = DateTime.UtcNow,
                Type = SynchronizationLogType.DeviceUnexpectedException,
                Log = $@"<font color=""red"">{exception.StackTrace}</font>"
            }, Guid.NewGuid());

            return this.Ok();
        }

    }
}
