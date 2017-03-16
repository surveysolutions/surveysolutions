using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        private readonly IPlainStorageAccessor<DeviceSyncInfo> deviceSyncInfoRepository;

        public DevicesApiV2Controller(
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository,
            IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<DeviceSyncInfo> deviceSyncInfoRepository,
            HqUserManager userManager) : base(
                authorizedUser: authorizedUser,
                syncVersionProvider: syncVersionProvider,
                commandService: commandService,
                devicesRepository: devicesRepository,
                userManager: userManager)
        {
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
        }

        [HttpGet]
        public override HttpResponseMessage CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        public override HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version) => base.LinkCurrentInterviewerToDevice(id, version);

        [HttpPost]
        public IHttpActionResult Info(DeviceInfoApiView info)
        {
            var deviceLocation = info.DeviceLocation;
            this.deviceSyncInfoRepository.Store(new DeviceSyncInfo
            {
                InterviewerId = this.authorizedUser.Id,
                DeviceId = info.DeviceId,
                LastAppUpdatedDate = info.LastAppUpdatedDate,
                DeviceModel = info.DeviceModel,
                DeviceType = info.DeviceType,
                AndroidVersion = info.AndroidVersion,
                DeviceLanguage = info.DeviceLanguage,
                DBSizeInfo = info.DBSizeInfo,
                AndroidSdkVersion = info.AndroidSdkVersion,
                DeviceDate = info.DeviceDate,
                AppVersion = info.AppVersion,
                MobileSignalStrength = info.MobileSignalStrength,
                AppOrientation = info.AppOrientation,
                MobileOperator = info.MobileOperator,
                NetworkSubType = info.NetworkSubType,
                NetworkType = info.NetworkType,
                BatteryChargePercent = info.BatteryChargePercent,
                BatteryPowerSource = info.BatteryPowerSource,
                DeviceLocationLat = deviceLocation?.Latitude,
                DeviceLocationLong = deviceLocation?.Longitude,
                NumberOfStartedInterviews = info.NumberOfStartedInterviews,
                RAMFreeInBytes = info.RAMInfo?.Free,
                RAMTotalInBytes = info.RAMInfo?.Total,
                StorageFreeInBytes = info.StorageInfo?.Free,
                StorageTotalInBytes = info.StorageInfo?.Total
            }, Guid.NewGuid().FormatGuid());

            return this.Ok();
        }
    }
}