using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DeviceSyncInfoRepository : IDeviceSyncInfoRepository
    {
        private readonly HQPlainStorageDbContext dbContext;
        public DeviceSyncInfoRepository(HQPlainStorageDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddOrUpdate(DeviceSyncInfo deviceSyncInfo)
        {
            if (deviceSyncInfo.Statistics != null)
                deviceSyncInfo.StatisticsId = dbContext.SyncStatistics.Add(deviceSyncInfo.Statistics).Id;
            dbContext.DeviceSyncInfo.AddOrUpdate(deviceSyncInfo);
            dbContext.SaveChanges();
        }

        public DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId);

        public DateTime? GetLastSyncronizationDate(Guid interviewerId) => this.dbContext.DeviceSyncInfo.OrderByDescending(di => di.Id)
            .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId)?.SyncDate;

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId &&
                    deviceInfo.StatisticsId != null
                    && (deviceInfo.Statistics.DownloadedInterviewsCount > 0
                        || deviceInfo.Statistics.UploadedInterviewsCount > 0
                        || deviceInfo.Statistics.DownloadedQuestionnairesCount > 0
                        || deviceInfo.Statistics.RejectedInterviewsOnDeviceCount > 0));

        public double? GetAverageSynchronizationSpeedInBytesPerSeconds(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(d => d.SyncDate)
                .Where(d => d.InterviewerId == interviewerId && d.StatisticsId != null)        
                .Take(5).Average(info => (double?)info.Statistics.TotalConnectionSpeed);

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);

        public int GetSuccessSynchronizationsCount(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId != null);

        public int GetFailedSynchronizationsCount(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);

        public SynchronizationActivity GetSynchronizationActivity(Guid interviewerId, string deviceId)
        {
            var limitByNumberOfDays = DateTime.UtcNow.AddDays(-7);

            var dbDaysGroupedBySyncDate = this.dbContext.DeviceSyncInfo
                .Where(syncInfo => syncInfo.InterviewerId == interviewerId && syncInfo.DeviceId == deviceId && syncInfo.SyncDate > limitByNumberOfDays)
                .Select(syncInfo => new DbDay
                {
                    Statistics = syncInfo.Statistics == null ? null : new DbDayStatistics
                    {
                        DownloadedQuestionnairesCount = syncInfo.Statistics.DownloadedQuestionnairesCount,
                        DownloadedInterviewsCount = syncInfo.Statistics.DownloadedInterviewsCount,
                        UploadedInterviewsCount = syncInfo.Statistics.UploadedInterviewsCount,
                        NewInterviewsOnDeviceCount = syncInfo.Statistics.NewInterviewsOnDeviceCount
                    },
                    SyncDate = syncInfo.SyncDate
                })
                .OrderBy(syncInfo=> syncInfo.SyncDate)
                .ToList()
                .GroupBy(syncInfo => syncInfo.SyncDate.ToString("MMM dd"));

            return new SynchronizationActivity
            {
                Days = dbDaysGroupedBySyncDate.Select(x => ToSyncDay(x.Key, x.ToArray())).ToArray()
            };
        }

        private static SyncDay ToSyncDay(string dayAndMonth, DbDay[] dbDays) => new SyncDay
        {
            Day = dayAndMonth,
            Quarters = new[]
            {
                ToSyncDayQuarter(dbDays.Where(x => x.SyncDate.Hour >= 0 && x.SyncDate.Hour < 6)),
                ToSyncDayQuarter(dbDays.Where(x => x.SyncDate.Hour >= 6 && x.SyncDate.Hour < 12)),
                ToSyncDayQuarter(dbDays.Where(x => x.SyncDate.Hour >= 12 && x.SyncDate.Hour < 18)),
                ToSyncDayQuarter(dbDays.Where(x => x.SyncDate.Hour >= 18 && x.SyncDate.Hour < 24))
            }
        };

        private static SyncDayQuarter ToSyncDayQuarter(IEnumerable<DbDay> quarter) => new SyncDayQuarter
        {
            NewInterviewsOnDeviceCount = quarter.Where(x => x.Statistics != null).Select(x => x.Statistics.NewInterviewsOnDeviceCount).DefaultIfEmpty().Max(),
            DownloadedInterviewsCount = quarter.Where(x => x.Statistics != null).Sum(x => x.Statistics.DownloadedInterviewsCount),
            DownloadedQuestionnairesCount = quarter.Where(x => x.Statistics != null).Sum(x => x.Statistics.DownloadedQuestionnairesCount),
            UploadedInterviewsCount = quarter.Where(x => x.Statistics != null).Sum(x => x.Statistics.UploadedInterviewsCount),
            FailedSynchronizationsCount = quarter.Count(x => x.Statistics == null),
            SynchronizationsWithoutChangesCount = quarter.Where(x => x.Statistics != null)
                .Count(x =>
                    x.Statistics.DownloadedQuestionnairesCount == 0 &&
                    x.Statistics.DownloadedInterviewsCount == 0 &&
                    x.Statistics.UploadedInterviewsCount == 0)
        };

        public class DbDay
        {
            public DateTime SyncDate { get; set; }
            public DbDayStatistics Statistics { get; set; }
        }

        public class DbDayStatistics
        {
            public int DownloadedInterviewsCount { get; set; }
            public int DownloadedQuestionnairesCount { get; set; }
            public int UploadedInterviewsCount { get; set; }
            public int NewInterviewsOnDeviceCount { get; set; }
        }
    }
}