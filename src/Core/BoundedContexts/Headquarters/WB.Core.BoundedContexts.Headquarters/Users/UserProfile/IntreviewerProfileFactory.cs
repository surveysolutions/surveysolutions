using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Fetching;
using WB.UI.Shared.Web.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserProfile
{
    public class InterviewerProfileFactory : IInterviewerProfileFactory
    {
        private readonly IUserRepository userManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IInterviewFactory interviewFactory;
        private readonly IAuthorizedUser currentUser;
        private readonly IQRCodeHelper qRCodeHelper;
        private readonly IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage;
        private readonly IVirtualPathService pathService;
        
        public InterviewerProfileFactory(
            IUserRepository userManager,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
            IDeviceSyncInfoRepository deviceSyncInfoRepository, 
            IInterviewerVersionReader interviewerVersionReader, 
            IInterviewFactory interviewFactory,
            IAuthorizedUser currentUser,
            IQRCodeHelper qRCodeHelper,
            IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage, 
            IVirtualPathService pathService)
        {
            this.userManager = userManager;
            this.interviewRepository = interviewRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
            this.interviewFactory = interviewFactory;
            this.currentUser = currentUser;
            this.qRCodeHelper = qRCodeHelper;
            this.profileSettingsStorage = profileSettingsStorage;
            this.pathService = pathService;
        }

        public InterviewerPoints GetInterviewerCheckInPoints(Guid interviewerId)
        {
            InterviewGpsAnswerWithTimeStamp[] points = interviewFactory.GetGpsAnswersForInterviewer(interviewerId);

            var checkInPoints = points
                .Where(HasAccessToInterview)
                .Where(p => !p.Idenifying)
                .GroupBy(x => new { x.Latitude, x.Longitude })
                .Select(x => new InterviewerPoint
                {
                    Latitude = x.Key.Latitude,
                    Longitude = x.Key.Longitude,
                    Timestamp = x.Min(p => p.Timestamp),
                    InterviewIds = x.Select(point => point.InterviewId).Distinct().ToList(),
                    Colors = x.Select(point => point.Status).Select(StatusToColor).Distinct().OrderBy(c => c).ToArray(),
                })
                .OrderBy(x => x.Timestamp)
                .ToList();
            
            for (var index = 0; index < checkInPoints.Count; index++)
            {
                checkInPoints[index].Index = index + 1;
            }

            var targetLocations =  points
                .Where(HasAccessToInterview)
                .Where(p => p.Idenifying)
                .Select(x => new InterviewerPoint
                {
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Timestamp = x.Timestamp,
                    InterviewIds = new List<Guid> { x.InterviewId }
                })
                .ToList();


            return new InterviewerPoints
            {
                CheckInPoints = checkInPoints,
                TargetLocations = targetLocations
            };
        }

        public async Task<InterviewerTrafficUsage> GetInterviewerTrafficUsageAsync(Guid interviewerId)
        {
            List<InterviewerDailyTrafficUsage> interviewerDailyTrafficUsages = this.deviceSyncInfoRepository.GetTrafficUsageForInterviewer(interviewerId);

            if (interviewerDailyTrafficUsages.Count == 0) return null;

            var maxDailyUsage = interviewerDailyTrafficUsages.Max(x => x.DownloadedBytes + x.UploadedBytes);

            var months = interviewerDailyTrafficUsages.Select(x => new DateTime(x.Year, x.Month, 1)).Distinct().OrderBy(x => x).ToList();
            var formattedDates = new Dictionary<DateTime, string>();

            for (int i = 0; i < months.Count; i++)
            {
                var dateFormat = (i > 0 && months[i - 1].Year != months[i].Year) ||
                                 (i < months.Count - 1 && months[i + 1].Year != months[i].Year)
                    ? "MMM yy"
                    : "MMM";
                
                formattedDates.Add(months[i], months[i].ToString(dateFormat, CultureInfo.CurrentUICulture));
            }

            var monthlyTrafficUsages= interviewerDailyTrafficUsages.GroupBy(x => new DateTime(x.Year, x.Month, 1))
                .Select(x => new InterviewerMonthlyTrafficUsageView
                {
                    Month = formattedDates[x.Key],
                    Date = x.Key,
                    DailyUsage = x.Select(d =>
                        {
                            var upInPer = (int) Math.Floor(100 * (double) d.UploadedBytes / maxDailyUsage);
                            var downInPer = (int) Math.Floor(100 * (double) d.DownloadedBytes / maxDailyUsage);
                            return new InterviewerDailyTrafficUsageView
                            {
                                Day = d.Day,
                                Up = d.UploadedBytes.InKb(),
                                Down = d.DownloadedBytes.InKb(),
                                UpInPer = d.UploadedBytes > 0 ? Math.Max(1, upInPer) : 0,
                                DownInPer = d.DownloadedBytes > 0 ? Math.Max(1, downInPer) : 0,
                            };
                        })
                        .OrderBy(d => d.Day)
                        .ToList()
                })
                .ToList();

            foreach (var monthlyTrafficUsage in monthlyTrafficUsages)
            {
                var daysWithData = monthlyTrafficUsage.DailyUsage.Count();
                if (daysWithData >= 3) continue;

                var daysInMonth = DateTime.DaysInMonth(monthlyTrafficUsage.Date.Year, monthlyTrafficUsage.Date.Month);
                var days = Enumerable.Range(1, daysInMonth).Except(monthlyTrafficUsage.DailyUsage.Select(x => x.Day)).OrderBy(x => x).ToList();

                monthlyTrafficUsage.DailyUsage.AddRange(
                    days.Skip(Math.Max(0, days.Count - 3 + daysWithData)).Take(3 - daysWithData)
                        .Select(x => new InterviewerDailyTrafficUsageView
                        {
                            Day = x
                        }));

                monthlyTrafficUsage.DailyUsage = monthlyTrafficUsage.DailyUsage.OrderBy(x => x.Day).ToList();
            }

            var totalTrafficUsed = await this.deviceSyncInfoRepository.GetTotalTrafficUsageForInterviewer(interviewerId);
            return new InterviewerTrafficUsage
            {
                TrafficUsages = monthlyTrafficUsages,
                TotalTrafficUsed = totalTrafficUsed.InKb(),
                MaxDailyUsage = maxDailyUsage.InKb()
            };
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

            var supervisor = await this.userManager.FindByIdAsync(interviewer.WorkspaceProfile.SupervisorId.Value);

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastSuccessByInterviewerId(userId);
            var registeredDeviceCount = this.deviceSyncInfoRepository.GetRegisteredDeviceCount(userId);
            var trafficUsed = (await this.deviceSyncInfoRepository.GetTotalTrafficUsageForInterviewer(userId)).InKb();
            var lastFailedDeviceInfo = this.deviceSyncInfoRepository.GetLastFailedByInterviewerId(userId);
            var totalSuccessSynchronizationCount =
                this.deviceSyncInfoRepository.GetSuccessSynchronizationsCount(interviewer.Id);
            var totalFailedSynchronizationCount =
                this.deviceSyncInfoRepository.GetFailedSynchronizationsCount(interviewer.Id);
            var lastSynchronizationDate =
                this.deviceSyncInfoRepository.GetLastSynchronizationDate(interviewer.Id);
            var averageSyncSpeedBytesPerSecond =
                this.deviceSyncInfoRepository.GetAverageSynchronizationSpeedInBytesPerSeconds(interviewer.Id);

            int? interviewerApkVersion = await interviewerVersionReader.InterviewerBuildNumber().ConfigureAwait(false);
            
            InterviewerProfileModel profile = 
                this.FillInterviewerProfileForExport(new InterviewerProfileModel(), 
                    interviewer, supervisor, lastSuccessDeviceInfo,
                    lastFailedDeviceInfo, 
                    totalSuccessSynchronizationCount,
                    totalFailedSynchronizationCount,
                    lastSynchronizationDate,
                    averageSyncSpeedBytesPerSecond,
                    trafficUsed, interviewerApkVersion) as InterviewerProfileModel;

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
            profile.RegistredDevicesCount = registeredDeviceCount;
            profile.HasAnyGpsAnswerForInterviewer = interviewFactory.HasAnyGpsAnswerForInterviewer(userId);

            profile.SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration();
            profile.QRCodeAsBase64String = qRCodeHelper.GetQRCodeAsBase64StringSrc(
                JsonConvert.SerializeObject(new FinishInstallationInfo
                {
                    Url = pathService.GetAbsolutePath("/"),
                    Login = profile.InterviewerName
                }), 250, 250);

            if (currentUser.IsInterviewer)
            {
                var profileSettings = this.profileSettingsStorage.GetById(AppSetting.ProfileSettings);
                profile.IsModifiable = profileSettings?.AllowInterviewerUpdateProfile ?? false;
            }
            else
            {
                profile.IsModifiable = true;
            }

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
        public async Task<ReportView> GetInterviewersReport(Guid[] interviewersIdsToExport)
        {
            int? interviewerApkVersion = await interviewerVersionReader.InterviewerBuildNumber()
                .ConfigureAwait(false);
            
            var userProfiles = GetProfilesForInterviewers(interviewersIdsToExport, interviewerApkVersion);

            return new ReportView
            {
                Headers = new[]
                {
                    "i_name",
                    "i_id",
                    "i_supervisorName",
                    "i_fullName",
                    "i_email",
                    "i_phone",
                    "i_lastLoginDate",
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
                    "s_traffic_used",
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
                    x.FullName,
                    x.Email,
                    x.Phone,
                    x.LastLoginDate?.ToString("s", CultureInfo.InvariantCulture) ?? "",
                    x.InterviewerAppVersion,
                    x.HasUpdateForInterviewerApp,
                    x.DeviceAssignmentDate?.ToString("yyyy-MM-dd"),
                    x.TotalNumberOfSuccessSynchronizations,
                    x.TotalNumberOfFailedSynchronizations,
                    x.AverageSyncSpeedBytesPerSecond,
                    x.LastCommunicationDate?.ToString("s", CultureInfo.InvariantCulture) ?? "",
                    x.DeviceId,
                    x.DeviceSerialNumber,
                    x.DeviceType,
                    x.DeviceManufacturer,
                    x.DeviceModel,
                    x.DeviceBuildNumber,
                    x.TrafficUsed,
                    x.DeviceLanguage,
                    x.AndroidVersion,
                    x.LastSurveySolutionsUpdatedDate?.ToString("s", CultureInfo.InvariantCulture) ?? "",
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
                    x.ServerTimeAtTheBeginningOfSync?.ToString("s", CultureInfo.InvariantCulture) ?? "",
                    x.TabletTimeAtTeBeginningOfSync?.ToString("s", CultureInfo.InvariantCulture) ?? "",
                    x.ConnectionType,
                    x.ConnectionSubType,
                    x.QuestionnairesReceived,
                    x.AssignmentsReceived,
                    x.CompletedInterviewsReceivedFromInterviewer,
                    x.AssignmentsThatHaveBeenStarted,
                    x.NewInterviewsOnDevice,
                    x.RejectedInterviewsOnDevice
                }).ToArray()
            };
        }

        private IEnumerable<InterviewerProfileToExport> GetProfilesForInterviewers(Guid[] interviewersIds, int? hqInterviewerVersion)
        {
            var interviewerProfiles = this.userManager.Users.Where(x => interviewersIds.Contains(x.Id))
                .Fetch(x => x.WorkspaceProfile)
                .Where(x => x!= null)
                .ToList();

            var supervisorIds = interviewerProfiles
                .Where(x => x.WorkspaceProfile.SupervisorId.HasValue)
                .Select(x => x.WorkspaceProfile.SupervisorId.Value)
                .ToArray();
            
            var supervisorsProfiles = this.userManager.Users.Where(x => supervisorIds.Contains(x.Id))
                .Where(x => x != null)
                .ToDictionary(x => x.Id, x => x);

            var deviceSyncInfos = this.deviceSyncInfoRepository
                .GetLastSyncByInterviewersList(interviewersIds)
                .ToDictionary(x => x.InterviewerId, x => x);

            var deviceFailedSyncInfos = this.deviceSyncInfoRepository
                .GetLastFailedByInterviewerIds(interviewersIds)
                .ToDictionary(x => x.InterviewerId, x => x);

            var syncStats = this.deviceSyncInfoRepository.GetSynchronizationsStats(interviewersIds);
            var averageSynchronizationSpeed = this.deviceSyncInfoRepository.GetAverageSynchronizationSpeedInBytesPerSeconds(interviewersIds);

            var trafficUsages = this.deviceSyncInfoRepository.GetInterviewersTrafficUsage(interviewersIds);

            return interviewerProfiles
                .Select(interviewer => FillInterviewerProfileForExport(
                        new InterviewerProfileToExport(), 
                        interviewer, 
                        interviewer.WorkspaceProfile.SupervisorId.HasValue? supervisorsProfiles.GetOrNull(interviewer.WorkspaceProfile.SupervisorId.Value) : null,
                        deviceSyncInfos.GetOrNull(interviewer.Id),
                        deviceFailedSyncInfos.GetOrNull(interviewer.Id),
                        syncStats.GetOrNull(interviewer.Id)?.SuccessSynchronizationsCount ?? 0,
                        syncStats.GetOrNull(interviewer.Id)?.FailedSynchronizationsCount ?? 0,
                        syncStats.GetOrNull(interviewer.Id)?.LastSynchronizationDate,
                        averageSynchronizationSpeed.ContainsKey(interviewer.Id)? averageSynchronizationSpeed[interviewer.Id] : (double?)null,
                        trafficUsages.ContainsKey(interviewer.Id) ? trafficUsages[interviewer.Id] : 0,
                        hqInterviewerVersion))
                .OrderBy(x=> x.InterviewerName);
        }

        //is used in bulk operations
        //all db queries inside could affect performance
        private InterviewerProfileToExport FillInterviewerProfileForExport(InterviewerProfileToExport profile,
            HqUser interviewer, 
            HqUser supervisor, 
            DeviceSyncInfo lastSuccessDeviceInfo,
            DeviceSyncInfo lastFailedDeviceInfo,
            int totalSuccessSynchronizationCount,
            int totalFailedSynchronizationCount,
            DateTime? lastSynchronizationDate,
            double? averageSyncSpeedBytesPerSecond,
            long trafficUsed,
            int? hqInterviewerVersion)
        {
            var interviewerId = interviewer.Id;

            string supervisorName = String.Empty;
            Guid supervisorId = Guid.Empty;

            if (supervisor != null)
            {
                supervisorId = supervisor.Id;
                supervisorName = supervisor.UserName;
            }

            var hasUpdateForInterviewerApp = false;
            if (lastSuccessDeviceInfo != null)
            {
                hasUpdateForInterviewerApp = hqInterviewerVersion.HasValue &&
                                             hqInterviewerVersion.Value > lastSuccessDeviceInfo.AppBuildVersion;
            }

            profile.Email = interviewer.Email;
            profile.IsArchived = interviewer.IsArchived;
            profile.FullName = interviewer.FullName;
            profile.Phone = interviewer.PhoneNumber;
            profile.InterviewerName = interviewer.UserName;
            profile.InterviewerId = interviewer.Id;

            profile.SupervisorName = supervisorName;
            profile.SupervisorId = supervisorId;
            profile.DeviceAssignmentDate = interviewer.WorkspaceProfile.DeviceRegistrationDate;
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

            profile.LastCommunicationDate = lastSynchronizationDate;
            profile.HasDeviceInfo = lastSuccessDeviceInfo != null;
            if (lastSuccessDeviceInfo == null)
                return profile;

            profile.InterviewerAppVersion = interviewer.WorkspaceProfile?.DeviceAppVersion ?? lastSuccessDeviceInfo.AppVersion;
            profile.DeviceId = interviewer.WorkspaceProfile?.DeviceId ?? lastSuccessDeviceInfo.DeviceId;
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
            profile.AssignmentsReceived = lastSuccessDeviceInfo.Statistics?.NewAssignmentsCount ?? 0;
            profile.CompletedInterviewsReceivedFromInterviewer = lastSuccessDeviceInfo.Statistics?.UploadedInterviewsCount ?? 0;
            profile.NewInterviewsOnDevice = lastSuccessDeviceInfo.Statistics?.NewInterviewsOnDeviceCount ?? 0;
            profile.RejectedInterviewsOnDevice = lastSuccessDeviceInfo.Statistics?.RejectedInterviewsOnDeviceCount ?? 0;

            profile.TrafficUsed = trafficUsed;
            profile.LastLoginDate = interviewer.LastLoginDate;

            return profile;
        }
    }
}
