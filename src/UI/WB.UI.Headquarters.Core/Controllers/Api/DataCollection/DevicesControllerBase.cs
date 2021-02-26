using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional.Internal;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class DevicesControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IUserToDeviceService userToDeviceService;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IUserRepository userRepository;
        private readonly IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository;

        protected DevicesControllerBase(
            IAuthorizedUser authorizedUser,
            ISyncProtocolVersionProvider syncVersionProvider,
            IUserToDeviceService userToDeviceService,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IUserRepository userRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository)
        {
            this.authorizedUser = authorizedUser;
            this.syncVersionProvider = syncVersionProvider;
            this.userToDeviceService = userToDeviceService;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.userRepository = userRepository;
            this.syncLogRepository = syncLogRepository;
        }
        
        public virtual IActionResult CanSynchronize(string id, int version)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();
            int supervisorShiftVersionNumber = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (version != supervisorRevisionNumber || version < supervisorShiftVersionNumber)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired);
            }

            var linkedDevice = this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id);

            return linkedDevice != id ? (IActionResult)Forbid() : Ok();
        }
        
        public virtual async Task<IActionResult> LinkCurrentResponsibleToDevice(string id, int version)
        {
            await this.userToDeviceService.LinkDeviceToUserAsync(this.authorizedUser.Id, id);

            return this.Ok();
        }

        public virtual async Task<IActionResult> Info(DeviceInfoApiView info)
        {
            var deviceLocation = info.DeviceLocation;

            var user = await this.userRepository.FindByIdAsync(this.authorizedUser.Id);

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

            user.WorkspacesProfile.DeviceAppBuildVersion = info.AppBuildVersion;
            user.WorkspacesProfile.DeviceAppVersion = info.AppVersion;
            user.WorkspacesProfile.StorageFreeInBytes = info.StorageInfo?.Free;

            return this.Ok();
        }

        public virtual IActionResult Statistics(SyncStatisticsApiView statistics)
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
            return new JsonResult(DateTime.UtcNow.ToEpochTime());
        }

        public virtual IActionResult UnexpectedException(UnexpectedExceptionApiView exception)
        {
            var deviceId = this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id);

            this.syncLogRepository.Store(new SynchronizationLogItem
            {
                DeviceId = deviceId,
                InterviewerId = this.authorizedUser.Id,
                InterviewerName = this.authorizedUser.UserName,
                LogDate = DateTime.UtcNow,
                Type = SynchronizationLogType.DeviceUnexpectedException,
                Log = $@"<pre><font color=""red"">{exception.StackTrace.Replace("\r\n", "<br />")}</font></pre>"
            }, Guid.NewGuid());

            return this.Ok();
        }

    }
}
