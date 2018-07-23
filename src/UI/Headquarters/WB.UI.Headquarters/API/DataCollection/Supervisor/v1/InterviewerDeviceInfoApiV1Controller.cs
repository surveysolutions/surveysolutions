using System;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewerDeviceInfoApiV1Controller : ApiController
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly HqUserManager userManager;

        public InterviewerDeviceInfoApiV1Controller(IDeviceSyncInfoRepository deviceSyncInfoRepository,
            HqUserManager userManager)
        {
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post(DeviceInfoApiView info)
        {
            if (info == null)
                return this.BadRequest("Server cannot accept empty package content.");

            if (info.UserId == null)
                return this.BadRequest("Invalid user Id was specified.");
            
            var deviceLocation = info.DeviceLocation;

            var userId = info.UserId;
            DateTime deviceInfoReceivedDate = info.ReceivedDate.Value.UtcDateTime;

            var user = await this.userManager.FindByIdAsync(userId.Value);

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

            user.Profile.DeviceAppBuildVersion = info.AppBuildVersion;
            user.Profile.DeviceAppVersion = info.AppVersion;

            await this.userManager.UpdateAsync(user);

            return this.Ok();
        }
    }
}
