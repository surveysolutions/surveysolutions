using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class DeviceInterviewersReport : IDeviceInterviewersReport
    {
        private readonly IUserRepository userRepository;

        public DeviceInterviewersReport(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<DeviceInterviewersReportView> LoadAsync(DeviceByInterviewersReportInputModel input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var items = from i in userRepository.Users
                let lastSync = userRepository.DeviceSyncInfos.Where(ds => ds.InterviewerId == i.Id).OrderByDescending(ds => ds.Id).FirstOrDefault()
                let anyAssignmentReceived = userRepository.DeviceSyncInfos.Any(ds => ds.InterviewerId == i.Id && ds.Statistics.DownloadedQuestionnairesCount > 0)
                let anyInterviewsUploaded = userRepository.DeviceSyncInfos.Any(ds => ds.InterviewerId == i.Id && ds.Statistics.UploadedInterviewsCount > 0)
                let hasTwoTablets = userRepository.DeviceSyncInfos.Where(ds => ds.InterviewerId == i.Id).Select(ds => ds.DeviceId).Distinct().Count() > 1
                where i.Profile.SupervisorId != null && i.IsArchived == false
                select new
                {
                    UserId = i.Id,
                    NeverSynched = lastSync == null ? 1 : 0,
                    SupervisorId = i.Profile.SupervisorId,
                    anyAssignmentReceived = anyAssignmentReceived ? 0 : 1,
                    noInterviewsUploaded = anyInterviewsUploaded ? 0 : 1,
                    hasTwoTablets = hasTwoTablets ? 1 : 0,
                    hasOldAndroidVersion = lastSync != null && lastSync.AndroidSdkVersion < InterviewerIssuesConstants.MinAndroidSdkVersion ? 1 : 0,
                    hasWrongTimeOnTablet = lastSync != null && Math.Abs((int)DbFunctions.DiffMinutes(lastSync.DeviceDate, lastSync.SyncDate)) > InterviewerIssuesConstants.MinutesForWrongTime ? 1 : 0,
                    hasLowStorage = lastSync != null && lastSync.StorageFreeInBytes < InterviewerIssuesConstants.LowMemoryInBytesSize ? 1 : 0
                };

            var lines = from i in items
                group i by i.SupervisorId
                into grouping
                join u in userRepository.Users on grouping.Key equals u.Id
                select new DeviceInterviewersReportLine
                {
                    TeamId = grouping.Key.Value,
                    TeamName = u.UserName,
                    NeverSynchedCount = grouping.Sum(x => x.NeverSynched),
                    NoQuestionnairesCount = grouping.Sum(x => x.anyAssignmentReceived),
                    NeverUploadedCount = grouping.Sum(x => x.noInterviewsUploaded),
                    ReassignedCount = grouping.Sum(x => x.hasTwoTablets),
                    OldAndroidCount = grouping.Sum(x => x.hasOldAndroidVersion),
                    WrongDateOnTabletCount = grouping.Sum(x => x.hasWrongTimeOnTablet),
                    LowStorageCount = grouping.Sum(x => x.hasLowStorage)
                };
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                lines = lines.Where(u => u.TeamName.ToLower().Contains(input.Filter.ToLower()));
            }

            return new DeviceInterviewersReportView
            {
                Items = await lines.OrderUsingSortExpression(input.Order).Take(input.PageSize).Skip(input.PageSize * input.Page).ToListAsync(),
                TotalCount = await lines.CountAsync()
            };
        }

        public async Task<ReportView> GetReportAsync(DeviceByInterviewersReportInputModel input)
        {
            var view = await this.LoadAsync(input);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEAMS, Report.COLUMN_NEVER_SYNCHED, Report.COLUMN_OLD_VERSION, Report.COLUMN_LESS_THAN_100MB_FREE_SPACE, Report.COLUMN_WRONG_TIME_ON_TABLET,
                    Report.COLUMN_ANDROID_4_4_OR_LOWER, Report.COLUMN_NO_ASSIGNMENTS_RECEIVED, Report.COLUMN_NEVER_UPLOADED, Report.COLUMN_TABLET_REASSIGNED
                },
                Data = view.Items.Select(x => new object[]
                {
                    x.TeamName, x.NeverSynchedCount, x.OutdatedCount, x.LowStorageCount, x.WrongDateOnTabletCount,
                    x.OldAndroidCount, x.NoQuestionnairesCount, x.NeverUploadedCount, x.ReassignedCount
                }).ToArray()
            };
        }
    }
}