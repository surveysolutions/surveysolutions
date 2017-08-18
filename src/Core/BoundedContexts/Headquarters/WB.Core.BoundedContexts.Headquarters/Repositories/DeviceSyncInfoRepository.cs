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
        {
            var result = this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId &&
                                              deviceInfo.StatisticsId != null
                                              && (deviceInfo.Statistics.DownloadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.UploadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.DownloadedQuestionnairesCount > 0
                                                  || deviceInfo.Statistics.RejectedInterviewsOnDeviceCount > 0));

            return result ?? this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId);
        }

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
            var toDay = DateTime.UtcNow;
            var fromDay = toDay.AddDays(-6);

            var deviceInfoByPeriod = this.dbContext.DeviceSyncInfo
                .Where(syncInfo => syncInfo.InterviewerId == interviewerId && syncInfo.DeviceId == deviceId &&
                                   syncInfo.SyncDate > fromDay)
                .Select(syncInfo => new DbDay
                {
                    Statistics = syncInfo.Statistics == null
                        ? null
                        : new DbDayStatistics
                        {
                            AllAssignmentsOnDeviceCount = syncInfo.Statistics.AssignmentsOnDeviceCount,
                            DownloadedAssignmentsCount = syncInfo.Statistics.NewAssignmentsCount,
                            UploadedInterviewsCount = syncInfo.Statistics.UploadedInterviewsCount
                        },
                    SyncDate = syncInfo.SyncDate
                })
                .OrderBy(syncInfo => syncInfo.SyncDate)
                .ToList();
                

            return new SynchronizationActivity
            {
                Days = this.GetDays(deviceInfoByPeriod, fromDay, toDay).ToArray()
            };
        }

        private IEnumerable<SyncDay> GetDays(List<DbDay> deviceInfoByPeriod, DateTime fromDay, DateTime toDay)
        {
            var syncDateFormat = "MMM dd";
            var deviceInfoGroupedBySyncDate = deviceInfoByPeriod
                .GroupBy(syncInfo => syncInfo.SyncDate.ToString(syncDateFormat))
                .ToDictionary(x => x.Key, x => x);

            for (int i = 0; i <= (toDay - fromDay).Days; i++)
            {
                var currentDay = fromDay.AddDays(i).ToString(syncDateFormat);
                
                yield return ToSyncDay(currentDay,
                    deviceInfoGroupedBySyncDate.ContainsKey(currentDay)
                        ? deviceInfoGroupedBySyncDate[currentDay].ToArray()
                        : Array.Empty<DbDay>());
            }
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
            DownloadedAssigmentsCount = quarter.Where(x => x.Statistics != null).Sum(x => x.Statistics.DownloadedAssignmentsCount),
            AllAssignmentsOnDeviceCount = quarter.Where(x => x.Statistics != null).Select(x => x.Statistics.AllAssignmentsOnDeviceCount).DefaultIfEmpty().Max(),
            UploadedInterviewsCount = quarter.Where(x => x.Statistics != null).Sum(x => x.Statistics.UploadedInterviewsCount),
            FailedSynchronizationsCount = quarter.Count(x => x.Statistics == null),
            SynchronizationsWithoutChangesCount = quarter.Where(x => x.Statistics != null)
                .Count(x =>x.Statistics.DownloadedAssignmentsCount == 0 &&
                           x.Statistics.UploadedInterviewsCount == 0)
        };

        public class DbDay
        {
            public DateTime SyncDate { get; set; }
            public DbDayStatistics Statistics { get; set; }
        }

        public class DbDayStatistics
        {
            public int UploadedInterviewsCount { get; set; }
            public int DownloadedAssignmentsCount { get; set; }
            public int AllAssignmentsOnDeviceCount { get; set; }
        }
    }
}