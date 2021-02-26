using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class InterviewerDeviceInfoApiV1Controller : ControllerBase
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IUserRepository userRepository;

        public InterviewerDeviceInfoApiV1Controller(IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IUserRepository userRepository)
        {
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.userRepository = userRepository;
        }

        [HttpPost]
        [Route("api/supervisor/v1/interviewerTabletInfos")]
        public async Task<IActionResult> Post([FromBody]DeviceInfoApiView info)
        {
            if (info == null)
                return this.BadRequest("Server cannot accept empty package content.");

            if (info.UserId == null)
                return this.BadRequest("Invalid user Id was specified.");
            
            var deviceLocation = info.DeviceLocation;

            var userId = info.UserId;
            DateTime deviceInfoReceivedDate = info.ReceivedDate.Value.UtcDateTime;

            var user = await this.userRepository.FindByIdAsync(userId.Value);

            if (user == null)
                return this.BadRequest("User was not found.");

            this.deviceSyncInfoRepository.AddOrUpdate(new DeviceSyncInfo
            {
                SyncDate = deviceInfoReceivedDate,
                InterviewerId = userId.Value,

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

            return this.Ok();
        }
    }
}
