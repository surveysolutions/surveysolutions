using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DeviceSyncInfoRepository : IDeviceSyncInfoRepository
    {
        private readonly UnitOfWork dbContext;

        public DeviceSyncInfoRepository(UnitOfWork dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void AddOrUpdate(DeviceSyncInfo deviceSyncInfo)
        {
            this.dbContext.Session.Save(deviceSyncInfo);
        }

        public DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId)
        {
            return this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId);
        }

        public DateTime? GetLastSyncronizationDate(Guid interviewerId)
        {
            return this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(di => di.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId)?.SyncDate;
        }

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId)
        {
            var result = this.dbContext.Session.Query<DeviceSyncInfo>()
                .OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId &&
                                              deviceInfo.Statistics != null
                                              && (deviceInfo.Statistics.DownloadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.UploadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.DownloadedQuestionnairesCount > 0
                                                  || deviceInfo.Statistics.RejectedInterviewsOnDeviceCount > 0));

            return result ?? this.GetLastSuccessByInterviewerId(interviewerId);
        }

        public Dictionary<Guid, long> GetInterviewersTrafficUsage(Guid[] interviewersIds)
        {
            var trafficUsage = this.dbContext.Session.Query<DeviceSyncInfo>().Where(x => interviewersIds.Contains(x.InterviewerId))
                .Select(x => new {x.InterviewerId, Traffic = x.Statistics.TotalDownloadedBytes + x.Statistics.TotalUploadedBytes})
                .GroupBy(x => x.InterviewerId)
                .Select(x => new {InterviewerId = x.Key, Traffic = x.Sum(s => (long?)s.Traffic) ?? 0})
                .ToList();

            return trafficUsage.ToDictionary(x => x.InterviewerId, x => x.Traffic);
        }

        public IEnumerable<DeviceSyncInfo> GetLastSyncByInterviewersList(Guid[] interviewerIds)
        {
            var syncWithNotEmptyStat =
                (from device in this.dbContext.Session.Query<DeviceSyncInfo>()
                where interviewerIds.Contains(device.InterviewerId)
                      && (device.Statistics != null &&
                          device.Statistics.DownloadedInterviewsCount +
                          device.Statistics.UploadedInterviewsCount +
                          device.Statistics.DownloadedQuestionnairesCount +
                          device.Statistics.RemovedInterviewsCount +
                          device.Statistics.RemovedAssignmentsCount +
                          device.Statistics.RejectedInterviewsOnDeviceCount > 0)
                group device by device.InterviewerId
                into grouping
                select new
                {
                    grouping.Key,
                    DeviceInfo = grouping.FirstOrDefault(g => g.Id == grouping.Max(d => d.Id))
                }).ToList();

            var lastSync = (from device in this.dbContext.Session.Query<DeviceSyncInfo>()
                where interviewerIds.Contains(device.InterviewerId)
                group device by device.InterviewerId
                into grouping
                select new
                {
                    grouping.Key,
                    DeviceInfo = grouping.FirstOrDefault(g => g.Id == grouping.Max(d => d.Id))
                }).ToList();

            foreach (var interviewerId in interviewerIds)
            {
                var notEmptyStat = syncWithNotEmptyStat.FirstOrDefault(x => x.Key == interviewerId);
                if (notEmptyStat != null)
                {
                    yield return notEmptyStat.DeviceInfo;
                    continue;
                }

                var lastStat = lastSync.FirstOrDefault(x => x.Key == interviewerId);
                if (lastStat!=null)
                    yield return lastStat.DeviceInfo;
            }
        }

        public int GetRegisteredDeviceCount(Guid interviewerId)
            => this.dbContext.Session.Query<DeviceSyncInfo>()
                .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId)
                .Select(info => info.DeviceId).Distinct().Count();

        public List<InterviewerDailyTrafficUsage> GetTrafficUsageForInterviewer(Guid interviewerId) =>
            this.dbContext.Session.Query<DeviceSyncInfo>()
                .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId)
                .GroupBy(x => x.SyncDate.Date, x => new
                {
                    Date = x.SyncDate,
                    TotalUploadedBytes = x.Statistics == null ? 0 : x.Statistics.TotalUploadedBytes,
                    TotalDownloadedBytes = x.Statistics == null ? 0 : x.Statistics.TotalDownloadedBytes
                })
                .OrderByDescending(x => x.Key)
                .Take(30)
                .ToList()
                .OrderBy(x => x.Key)
                .Select(x => new InterviewerDailyTrafficUsage
                {
                    DownloadedBytes = x.Sum(s => s.TotalDownloadedBytes),
                    UploadedBytes = x.Sum(s => s.TotalUploadedBytes),
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Day = x.Key.Day
                })
                .ToList();

        public async Task<long> GetTotalTrafficUsageForInterviewer(Guid interviewerId)
        {
            var totalTrafficUsed = await this.dbContext.Session.Query<DeviceSyncInfo>()
                .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId)
                .Select(x => new
                    {
                        TrafficUsed = x.Statistics.TotalUploadedBytes + x.Statistics.TotalDownloadedBytes
                    })
                .ToListAsync();


            return totalTrafficUsed.Sum(x => x.TrafficUsed);
        }

        public double? GetAverageSynchronizationSpeedInBytesPerSeconds(Guid interviewerId)
            => this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(d => d.SyncDate)
                .Where(d => d.InterviewerId == interviewerId && d.Statistics != null)        
                .Take(5).Average(info => (double?)info.Statistics.TotalConnectionSpeed);

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId)
            => this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.Statistics == null);

        public int GetSuccessSynchronizationsCount(Guid interviewerId)
            => this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo != null);

        public int GetFailedSynchronizationsCount(Guid interviewerId)
            => this.dbContext.Session.Query<DeviceSyncInfo>().OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.Statistics == null);
        
        public SynchronizationActivity GetSynchronizationActivity(Guid interviewerId)
        {
            var toDay = DateTime.UtcNow;
            var fromDay = toDay.AddDays(-6);

            var deviceInfoByPeriod = this.dbContext.Session.Query<DeviceSyncInfo>()
                .Where(syncInfo => syncInfo.InterviewerId == interviewerId && syncInfo.SyncDate > fromDay)
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
