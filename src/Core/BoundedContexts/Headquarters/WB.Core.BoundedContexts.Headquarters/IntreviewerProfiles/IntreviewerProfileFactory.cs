using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles
{
    public interface IInterviewerProfileFactory
    {
        Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid interviewerId);

        ReportView GetInterviewersReport(Guid[] interviewersIdsToExport);

        IEnumerable<InterviewerPoint> GetInterviewerCheckinPoints(Guid interviewerId);
    }

    public class InterviewerProfileFactory : IInterviewerProfileFactory
    {
        private readonly HqUserManager userManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IInterviewFactory interviewFactory;
        private readonly IAuthorizedUser currentUser;
        private readonly IQRCodeHelper qRCodeHelper;

        public InterviewerProfileFactory(
            HqUserManager userManager,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository, 
            IInterviewerVersionReader interviewerVersionReader, 
            IInterviewFactory interviewFactory,
            IAuthorizedUser currentUser,
            IQRCodeHelper qRCodeHelper)
        {
            this.userManager = userManager;
            this.interviewRepository = interviewRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
            this.interviewFactory = interviewFactory;
            this.currentUser = currentUser;
            this.qRCodeHelper = qRCodeHelper;
        }

        public IEnumerable<InterviewerPoint> GetInterviewerCheckinPoints(Guid interviewerId)
        {
            InterviewGpsAnswerWithTimeStamp[] points = interviewFactory.GetGpsAnswersForInterviewer(interviewerId);

            var checkinPoints = points
                .Where(p => HasAccessToInterview(p))
                .GroupBy(x => new { x.Latitude, x.Longitude })
                .Select(x => new InterviewerPoint
                {
                    Latitude = x.Key.Latitude,
                    Longitude = x.Key.Longitude,
                    Timestamp = x.Min(p => p.Timestamp),
                    InterviewIds = x.Select(point => point.InterviewId).Distinct().ToList(),
                    Colors = x.Select(point => point.Status).Select(StatusToColor).Distinct().OrderBy(c => c).ToArray()
                })
                .OrderBy(x => x.Timestamp)
                .ToArray();

            for (var index = 0; index < checkinPoints.Length; index++)
            {
                checkinPoints[index].Index = index + 1;
            }

            return checkinPoints;
        }

        private bool HasAccessToInterview(InterviewGpsAnswerWithTimeStamp answer)
        {
            if (currentUser.IsHeadquarter || currentUser.IsAdministrator)
                return true;

            var isSupervisor = currentUser.IsSupervisor;
            if (isSupervisor && HasSupervisorAccessToInterview(answer.Status))
                return true;

            return false;
        }

        private bool HasSupervisorAccessToInterview(InterviewStatus status)
        {
            return status == InterviewStatus.InterviewerAssigned
                || status == InterviewStatus.SupervisorAssigned
                || status == InterviewStatus.Completed
                || status == InterviewStatus.RejectedBySupervisor
                || status == InterviewStatus.RejectedByHeadquarters;
        }

        private string StatusToColor(InterviewStatus status)
        {
            switch (status)
            {
                case InterviewStatus.RejectedByHeadquarters:
                case InterviewStatus.RejectedBySupervisor:
                    return "red";

                case InterviewStatus.Completed:
                case InterviewStatus.ApprovedBySupervisor:
                case InterviewStatus.ApprovedByHeadquarters:
                    return "green";
                default:
                    return "blue";
            }
        }


        public async Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid userId)
        {
            var interviewer = await this.userManager.FindByIdAsync(userId);

            if (interviewer == null) return null;

            var supervisor = await this.userManager.FindByIdAsync(interviewer.Profile.SupervisorId.Value);

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastSuccessByInterviewerId(userId);
            var registredDeviceCount = this.deviceSyncInfoRepository.GetRegistredDeviceCount(userId);

            InterviewerProfileModel profile = 
                this.FillInterviewerProfileForExport(new InterviewerProfileModel(), interviewer, supervisor, lastSuccessDeviceInfo) as InterviewerProfileModel;

            if (profile == null) return null;

            var completedInterviewCount = this.interviewRepository
                .Query(interviews => interviews.Count(interview =>
                    interview.ResponsibleId == userId && interview.Status == InterviewStatus.Completed));

            var approvedByHqCount = this.interviewRepository
                .Query(interviews => interviews.Count(interview =>
                    interview.ResponsibleId == userId && interview.Status == InterviewStatus.ApprovedByHeadquarters));

            profile.WaitingInterviewsForApprovalCount = completedInterviewCount;
            profile.ApprovedInterviewsByHqCount = approvedByHqCount;
            profile.SynchronizationActivity = this.deviceSyncInfoRepository.GetSynchronizationActivity(userId);
            profile.RegistredDevicesCount = registredDeviceCount;
            profile.HasAnyGpsAnswerForInterviewer = interviewFactory.HasAnyGpsAnswerForInterviewer(userId);

            profile.SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration();
            profile.QRCodeAsBase64String = qRCodeHelper.GetQRCodeAsBase64StringSrc(
                JsonConvert.SerializeObject((new FinishInstallationInfo()
                {
                    Url = qRCodeHelper.GetBaseUrl(),
                    Login = profile.InterviewerName
                })), 250, 250);

            return profile;
        }

        /*
         * Conventions for indicators naming:
         * i – interviewer
         * t – tablet device
         * s – status
         * z – last synchronization
         * d – dashboard
         */
        public ReportView GetInterviewersReport(Guid[] interviewersIdsToExport)
        {
            var userProfiles = GetProfilesForInterviewers(interviewersIdsToExport);

            return new ReportView
            {
                Headers = new[]
                {
                    "i_name",
                    "i_id",
                    "i_supervisorName",
                    "s_appVersion",
                    "s_updateAvailable",
                    "s_linkedDate",
                    "i_nSyncSucc",
                    "i_nSyncFail",
                    "n_avgSyncSpeed",
                    "t_lastCommDate",
                    "t_id",
                    "t_serialNumber",
                    "t_deviceType",
                    "t_manufacturer",
                    "t_model",
                    "t_buildNumber",
                    "s_language",
                    "s_androidVersion",
                    "s_updatedDate",
                    "s_lastKnownLocationLat",
                    "s_lastKnownLocationLon",
                    "s_orientation",
                    "s_batteryStatus",
                    "s_powerSource",
                    "s_powerSaveMode",
                    "s_storageFree",
                    "t_storageTotal",
                    "s_RAMFree",
                    "t_RAMTotal",
                    "s_databaseSize",
                    "z_serverClockAtBeginLastSync",
                    "z_tabletClockAtBeginLastSync",
                    "z_connectionType",
                    "z_connectionSubType",
                    "z_questReceivedOnTablet",
                    "z_intervReceivedOnTablet",
                    "z_intervReceivedOnServer",
                    "d_numberAssignments",
                    "d_numberNewInterviews",
                    "d_numberRejectedInterviews"
                },
                Data = userProfiles.Where(x => x != null).Select(x => new object[]
                {
                    x.InterviewerName,
                    x.InterviewerId,
                    x.SupervisorName,
                    x.InterviewerAppVersion,
                    x.HasUpdateForInterviewerApp,
                    x.DeviceAssignmentDate,
                    x.TotalNumberOfSuccessSynchronizations,
                    x.TotalNumberOfFailedSynchronizations,
                    x.AverageSyncSpeedBytesPerSecond,
                    x.LastCommunicationDate,
                    x.DeviceId,
                    x.DeviceSerialNumber,
                    x.DeviceType,
                    x.DeviceManufacturer,
                    x.DeviceModel,
                    x.DeviceBuildNumber,
                    x.DeviceLanguage,
                    x.AndroidVersion,
                    x.LastSurveySolutionsUpdatedDate,
                    x.DeviceLocationOrLastKnownLocationLat,
                    x.DeviceLocationOrLastKnownLocationLon,
                    x.DeviceOrientation,
                    x.BatteryStatus,
                    x.BatteryPowerSource,
                    x.IsPowerSaveMode,
                    x.StorageFreeInBytes,
                    x.StorageTotalInBytes,
                    x.RamFreeInBytes,
                    x.RamTotalInBytes,
                    x.DatabaseSizeInBytes,
                    x.ServerTimeAtTheBeginningOfSync,
                    x.TabletTimeAtTeBeginningOfSync,
                    x.ConnectionType,
                    x.ConnectionSubType,
                    x.QuestionnairesReceived,
                    x.InterviewsReceived,
                    x.CompletedInterviewsReceivedFromInterviewer,
                    x.AssignmentsThatHaveBeenStarted,
                    x.NewInterviewsOnDevice,
                    x.RejectedInterviewsOnDevice
                }).ToArray()
            };
        }

        private IEnumerable<InterviewerProfileToExport> GetProfilesForInterviewers(Guid[] interviewersIds)
        {
            var interviewerProfiles = this.userManager.Users.Where(x => interviewersIds.Contains(x.Id))
                .Where(x => x!= null)
                .ToList();

            var supervisorIds = interviewerProfiles
                .Where(x => x.Profile.SupervisorId.HasValue)
                .Select(x => x.Profile.SupervisorId.Value)
                .ToArray();
            
            var supervisorsProfiles = this.userManager.Users.Where(x => supervisorIds.Contains(x.Id))
                .Where(x => x != null)
                .ToDictionary(x => x.Id, x => x);

            var deviceSyncInfos = this.deviceSyncInfoRepository
                .GetLastSyncByInterviewersList(interviewersIds)
                .ToDictionary(x => x.InterviewerId, x => x);

            return interviewerProfiles
                .Select(interviewer => FillInterviewerProfileForExport(
                        new InterviewerProfileToExport(), 
                        interviewer, 
                        interviewer.Profile.SupervisorId.HasValue? supervisorsProfiles.GetOrNull(interviewer.Profile.SupervisorId.Value) : null,
                    deviceSyncInfos.GetOrNull(interviewer.Id)));
        }

        private InterviewerProfileToExport FillInterviewerProfileForExport(InterviewerProfileToExport profile,
            HqUser interviewer, 
            HqUser supervisor, 
            DeviceSyncInfo lastSuccessDeviceInfo)
        {
            var interviewerId = interviewer.Id;

            string supervisorName = String.Empty;
            Guid supervisorId = Guid.Empty;

            if (supervisor != null)
            {
                supervisorId = supervisor.Id;
                supervisorName = supervisor.UserName;
            }

            var lastFailedDeviceInfo = this.deviceSyncInfoRepository.GetLastFailedByInterviewerId(interviewerId);
            var hasUpdateForInterviewerApp = false;

            if (lastSuccessDeviceInfo != null)
            {
                int? interviewerApkVersion = interviewerVersionReader.Version;
                hasUpdateForInterviewerApp = interviewerApkVersion.HasValue &&
                                             interviewerApkVersion.Value > lastSuccessDeviceInfo.AppBuildVersion;
            }

            var totalSuccessSynchronizationCount =
                this.deviceSyncInfoRepository.GetSuccessSynchronizationsCount(interviewerId);
            var totalFailedSynchronizationCount =
                this.deviceSyncInfoRepository.GetFailedSynchronizationsCount(interviewerId);

            var lastSyncronizationDate = this.deviceSyncInfoRepository.GetLastSyncronizationDate(interviewerId);
            var averageSyncSpeedBytesPerSecond =
                this.deviceSyncInfoRepository.GetAverageSynchronizationSpeedInBytesPerSeconds(interviewerId);

            profile.Email = interviewer.Email;
            profile.IsArchived = interviewer.IsArchived;
            profile.FullName = interviewer.FullName;
            profile.Phone = interviewer.PhoneNumber;
            profile.InterviewerName = interviewer.UserName;
            profile.InterviewerId = interviewer.Id;

            profile.SupervisorName = supervisorName;
            profile.SupervisorId = supervisorId;
            profile.DeviceAssignmentDate = interviewer.Profile.DeviceRegistrationDate;
            profile.HasUpdateForInterviewerApp = hasUpdateForInterviewerApp;
            profile.TotalNumberOfSuccessSynchronizations = totalSuccessSynchronizationCount;
            profile.TotalNumberOfFailedSynchronizations = totalFailedSynchronizationCount;
            profile.AverageSyncSpeedBytesPerSecond = averageSyncSpeedBytesPerSecond;

            profile.LastSuccessfulSync = new InterviewerProfileSyncStatistics
            {
                SyncDate = lastSuccessDeviceInfo?.SyncDate,
                HasStatistics = lastSuccessDeviceInfo != null,
                MobileOperator = lastSuccessDeviceInfo?.MobileOperator,
                NetworkSubType = lastSuccessDeviceInfo?.NetworkSubType,
                NetworkType = lastSuccessDeviceInfo?.NetworkType,
                TotalSyncDuration = lastSuccessDeviceInfo?.Statistics?.TotalSyncDuration ?? TimeSpan.Zero,
                TotalConnectionSpeed = lastSuccessDeviceInfo?.Statistics?.TotalConnectionSpeed ?? 0,
                TotalUploadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalUploadedBytes ?? 0,
                TotalDownloadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalDownloadedBytes ?? 0,
            };
            profile.LastFailedSync = new InterviewerProfileSyncStatistics
            {
                SyncDate = lastFailedDeviceInfo?.SyncDate,
                HasStatistics = lastFailedDeviceInfo != null,
                MobileOperator = lastFailedDeviceInfo?.MobileOperator,
                NetworkSubType = lastFailedDeviceInfo?.NetworkSubType,
                NetworkType = lastFailedDeviceInfo?.NetworkType,
                TotalSyncDuration = lastFailedDeviceInfo?.Statistics?.TotalSyncDuration ?? TimeSpan.Zero,
                TotalConnectionSpeed = lastFailedDeviceInfo?.Statistics?.TotalConnectionSpeed ?? 0,
                TotalUploadedBytes = lastFailedDeviceInfo?.Statistics?.TotalUploadedBytes ?? 0,
                TotalDownloadedBytes = lastFailedDeviceInfo?.Statistics?.TotalDownloadedBytes ?? 0,
            };

            profile.LastCommunicationDate = lastSyncronizationDate;
            profile.HasDeviceInfo = lastSuccessDeviceInfo != null;
            if (lastSuccessDeviceInfo == null)
                return profile;

            profile.InterviewerAppVersion = interviewer.Profile?.DeviceAppVersion ?? lastSuccessDeviceInfo.AppVersion;
            profile.DeviceId = interviewer.Profile?.DeviceId ?? lastSuccessDeviceInfo.DeviceId;
            profile.DeviceSerialNumber = lastSuccessDeviceInfo.DeviceSerialNumber;
            profile.DeviceType = lastSuccessDeviceInfo.DeviceType;
            profile.DeviceManufacturer = lastSuccessDeviceInfo.DeviceManufacturer;
            profile.DeviceModel = lastSuccessDeviceInfo.DeviceModel;
            profile.DeviceBuildNumber = lastSuccessDeviceInfo.DeviceBuildNumber;
            profile.DeviceLanguage = lastSuccessDeviceInfo.DeviceLanguage;
            profile.AndroidVersion = $"{lastSuccessDeviceInfo.AndroidVersion} {lastSuccessDeviceInfo.AndroidSdkVersionName}({lastSuccessDeviceInfo.AndroidSdkVersion})";
            profile.SurveySolutionsVersion = lastSuccessDeviceInfo.AppVersion;
            profile.LastSurveySolutionsUpdatedDate = lastSuccessDeviceInfo.LastAppUpdatedDate;
            profile.DeviceLocationOrLastKnownLocationLat = lastSuccessDeviceInfo.DeviceLocationLat;
            profile.DeviceLocationOrLastKnownLocationLon = lastSuccessDeviceInfo.DeviceLocationLong;
            profile.DeviceOrientation = lastSuccessDeviceInfo.AppOrientation;
            profile.BatteryStatus = lastSuccessDeviceInfo.BatteryChargePercent;
            profile.BatteryPowerSource = lastSuccessDeviceInfo.BatteryPowerSource;
            profile.IsPowerSaveMode = lastSuccessDeviceInfo.IsPowerInSaveMode;
            profile.StorageFreeInBytes = lastSuccessDeviceInfo.StorageFreeInBytes;
            profile.StorageTotalInBytes = lastSuccessDeviceInfo.StorageTotalInBytes;
            profile.RamFreeInBytes = lastSuccessDeviceInfo.RAMFreeInBytes;
            profile.RamTotalInBytes = lastSuccessDeviceInfo.RAMTotalInBytes;
            profile.DatabaseSizeInBytes = lastSuccessDeviceInfo.DBSizeInfo;
            profile.ServerTimeAtTheBeginningOfSync = lastSuccessDeviceInfo.SyncDate;
            profile.TabletTimeAtTeBeginningOfSync = lastSuccessDeviceInfo.DeviceDate;
            profile.ConnectionType = lastSuccessDeviceInfo.NetworkType;
            profile.ConnectionSubType = lastSuccessDeviceInfo.NetworkSubType;
            profile.AssignmentsThatHaveBeenStarted = lastSuccessDeviceInfo.NumberOfStartedInterviews;
            profile.QuestionnairesReceived = lastSuccessDeviceInfo.Statistics?.DownloadedQuestionnairesCount ?? 0;
            profile.InterviewsReceived = lastSuccessDeviceInfo.Statistics?.DownloadedInterviewsCount ?? 0;
            profile.CompletedInterviewsReceivedFromInterviewer = lastSuccessDeviceInfo.Statistics?.UploadedInterviewsCount ?? 0;
            profile.NewInterviewsOnDevice = lastSuccessDeviceInfo.Statistics?.NewInterviewsOnDeviceCount ?? 0;
            profile.RejectedInterviewsOnDevice = lastSuccessDeviceInfo.Statistics?.RejectedInterviewsOnDeviceCount ?? 0;

            return profile;
        }
    }
}
