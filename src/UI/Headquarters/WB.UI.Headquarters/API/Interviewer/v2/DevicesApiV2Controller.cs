using System;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(UserRoles.Interviewer)]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IDeviceExceptionRepository deviceExceptionRepository;

        public DevicesApiV2Controller(
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository,
            IAuthorizedUser authorizedUser,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IDeviceExceptionRepository deviceExceptionRepository,
            HqUserManager userManager) : base(
                authorizedUser: authorizedUser,
                syncVersionProvider: syncVersionProvider,
                commandService: commandService,
                devicesRepository: devicesRepository,
                userManager: userManager)
        {
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.deviceExceptionRepository = deviceExceptionRepository;
        }

        [HttpGet]
        public override HttpResponseMessage CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        public override HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version) => base.LinkCurrentInterviewerToDevice(id, version);

        [HttpPost]
        public IHttpActionResult Info(DeviceInfoApiView info)
        {
            var deviceLocation = info.DeviceLocation;
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
                SyncFinishDate = DateTime.UtcNow
            };
            this.deviceSyncInfoRepository.AddOrUpdate(deviceInfo);

            return this.Ok();
        }

        [HttpPost]
        public IHttpActionResult UnexpectedException(UnexpectedExceptionApiView exception)
        {
            this.deviceExceptionRepository.Add(new DeviceException
            {
                InterviewerId = this.authorizedUser.Id,
                DeviceId = this.authorizedUser.DeviceId,
                ExceptionDate = DateTime.UtcNow,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            });

            return this.Ok();
        }
    }
}