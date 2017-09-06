using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(UserRoles.Interviewer)]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository;
        private readonly HqUserManager userManager;

        public DevicesApiV2Controller(
            ISyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository,
            HqUserManager userManager) : base(
                authorizedUser: authorizedUser,
                syncVersionProvider: syncVersionProvider,
                userManager: userManager)
        {
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.syncLogRepository = syncLogRepository;
            this.userManager = userManager;
        }

        [HttpGet]
        public override HttpResponseMessage CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        public override HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version) => base.LinkCurrentInterviewerToDevice(id, version);

        [HttpPost]
        public async Task<IHttpActionResult> Info(DeviceInfoApiView info)
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

        [HttpPost]
        public IHttpActionResult Statistics(SyncStatisticsApiView statistics)
        {
            var deviceInfo = this.deviceSyncInfoRepository.GetLastByInterviewerId(this.authorizedUser.Id);
            deviceInfo.Statistics = new SyncStatistics
            {
                DownloadedInterviewsCount = statistics.DownloadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.DownloadedQuestionnairesCount,
                UploadedInterviewsCount = statistics.UploadedInterviewsCount,
                NewInterviewsOnDeviceCount = statistics.NewInterviewsOnDeviceCount,
                RejectedInterviewsOnDeviceCount = statistics.RejectedInterviewsOnDeviceCount,
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

        [HttpPost]
        public IHttpActionResult UnexpectedException(UnexpectedExceptionApiView exception)
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