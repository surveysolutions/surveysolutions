using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles
{
    public interface IInterviewerProfileFactory
    {
        Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid interviewerId);

        Task<ReportView> GetInterviewersReportAsync(Guid[] interviewersIdsToExport);
    }

    public class InterviewerProfileFactory : IInterviewerProfileFactory
    {
        private readonly HqUserManager userManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;

        public InterviewerProfileFactory(
            HqUserManager userManager,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository, IInterviewerVersionReader interviewerVersionReader)
        {
            this.userManager = userManager;
            this.interviewRepository = interviewRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
        }

        public async Task<InterviewerProfileModel> GetInterviewerProfileAsync(Guid userId)
        {
            InterviewerProfileModel profile = await this.FillInterviewerProfileForExportAsync(new InterviewerProfileModel(), userId) as InterviewerProfileModel;

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

            return profile;
        }

        public async Task<ReportView> GetInterviewersReportAsync(Guid[] interviewersIdsToExport)
        {
            var userProfiles = await GetProfilesForInterviewers(interviewersIdsToExport);

            return new ReportView
            {
                Headers = new[]
                {
                    "InterviewerName",
                    "InterviewerId",
                    "SupervisorName",
                    "InterviewerAppVersion",
                    "HasUpdateForInterviewerApp",
                    "DeviceAssignmentDate",
                    "TotalNumberOfSuccessSynchronizations",
                    "TotalNumberOfFailedSynchronizations",
                    "AverageSyncSpeedBytesPerSecond",
                    "LastCommunicationDate",
                    "DeviceID",
                    "DeviceSerialNumber",
                    "DeviceType",
                    "DeviceManufacturer",
                    "DeviceModel",
                    "DeviceBuildNumber",
                    "DeviceLanguage",
                    "AndroidVersion",
                    "SurveySolutionsVersion",
                    "LastSurveySolutionsUpdatedDate",
                    "DeviceLocationOrLastKnownLocationLat",
                    "DeviceLocationOrLastKnownLocationLon",
                    "DeviceOrientation",
                    "BatteryStatus",
                    "BatteryPowerSource",
                    "IsPowerSaveMode",
                    "StorageFreeInBytes",
                    "StorageTotalInBytes",
                    "RAMFreeInBytes",
                    "RAMTotalInBytes",
                    "DatabaseSizeInBytes",
                    "ServerTimeAtTheBeginningOfSync",
                    "TabletTimeAtTeBeginningOfSync",
                    "ConnectionType",
                    "ConnectionSubType",
                    "QuestionnairesReceived",
                    "InterviewsReceived",
                    "CompletedInterviewsReceivedFromInterviewer",
                    "AssignmentsThatHaveBeenStarted",
                    "NewInterviewsOnDevice",
                    "RejectedInterviewsOnDevice"
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
                    x.SurveySolutionsVersion,
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

        private Task<InterviewerProfileToExport[]> GetProfilesForInterviewers(Guid[] interviewersIds)
        {
            return Task.WhenAll(from interviewerId in interviewersIds
                select FillInterviewerProfileForExportAsync(new InterviewerProfileToExport(), interviewerId));
        }

        private async Task<InterviewerProfileToExport> FillInterviewerProfileForExportAsync(
            InterviewerProfileToExport profile, Guid interviewerId)
        {
            var interviewer = await this.userManager.FindByIdAsync(interviewerId).ConfigureAwait(false);

            if (interviewer == null) return null;

            string supervisorName = String.Empty;
            Guid supervisorId = Guid.Empty;

            if (interviewer.Profile.SupervisorId.HasValue)
            {
                supervisorId = interviewer.Profile.SupervisorId.Value;
                supervisorName = (await this.userManager.FindByIdAsync(supervisorId).ConfigureAwait(false)).UserName;
            }

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastSuccessByInterviewerId(interviewerId);
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
                MobileOperator = lastFailedDeviceInfo?.MobileOperator,
                NetworkSubType = lastSuccessDeviceInfo?.NetworkSubType,
                NetworkType = lastSuccessDeviceInfo?.NetworkType,
                TotalSyncDuration = lastSuccessDeviceInfo?.Statistics?.TotalSyncDuration ?? TimeSpan.Zero,
                TotalConnectionSpeed = lastSuccessDeviceInfo?.Statistics?.TotalConnectionSpeed ?? 0,
                TotalUploadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalUploadedBytes ?? 0,
                TotalDownloadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalDownloadedBytes ?? 0,
            };
            profile.LastFailedSync = new InterviewerProfileSyncStatistics
            {
                SyncDate = lastSuccessDeviceInfo?.SyncDate,
                HasStatistics = lastSuccessDeviceInfo != null,
                MobileOperator = lastFailedDeviceInfo?.MobileOperator,
                NetworkSubType = lastSuccessDeviceInfo?.NetworkSubType,
                NetworkType = lastSuccessDeviceInfo?.NetworkType,
                TotalSyncDuration = lastSuccessDeviceInfo?.Statistics?.TotalSyncDuration ?? TimeSpan.Zero,
                TotalConnectionSpeed = lastSuccessDeviceInfo?.Statistics?.TotalConnectionSpeed ?? 0,
                TotalUploadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalUploadedBytes ?? 0,
                TotalDownloadedBytes = lastSuccessDeviceInfo?.Statistics?.TotalDownloadedBytes ?? 0,
            };

            profile.LastCommunicationDate = lastSyncronizationDate;
            profile.HasDeviceInfo = lastSuccessDeviceInfo != null;
            if (lastSuccessDeviceInfo == null)
                return profile;

            profile.InterviewerAppVersion = lastSuccessDeviceInfo.AppVersion;
            profile.DeviceId = lastSuccessDeviceInfo.DeviceId;
            profile.DeviceSerialNumber = lastSuccessDeviceInfo.DeviceType;
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