using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DeviceSyncInfoRepository : IDeviceSyncInfoRepository
    {
        private readonly IPlainStorageAccessor<DeviceSyncInfo> dbContext;

        public DeviceSyncInfoRepository(IPlainStorageAccessor<DeviceSyncInfo> dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void AddOrUpdate(DeviceSyncInfo deviceSyncInfo)
        {
            this.dbContext.Store(deviceSyncInfo, null);
        }

        public DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId)
        {
            return this.dbContext.Query(_ => _.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId));
        }

        public DateTime? GetLastSynchronizationDate(Guid interviewerId)
        {
            return this.dbContext.Query(_ => _.OrderByDescending(di => di.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId)?.SyncDate);
        }

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId)
        {
            var result = this.dbContext.Query(_ => _
                .OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId &&
                                              (deviceInfo.Statistics.DownloadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.UploadedInterviewsCount > 0
                                                  || deviceInfo.Statistics.DownloadedQuestionnairesCount > 0
                                                  || deviceInfo.Statistics.RejectedInterviewsOnDeviceCount > 0)));

            return result ?? this.GetLastByInterviewerId(interviewerId);
        }

        public Dictionary<Guid, long> GetInterviewersTrafficUsage(Guid[] interviewersIds)
        {
            var trafficUsage = this.dbContext.Query(devices => devices.Where(x => interviewersIds.Contains(x.InterviewerId))
                .Select(x => new {x.InterviewerId, Traffic = x.Statistics.TotalDownloadedBytes + x.Statistics.TotalUploadedBytes})
                .GroupBy(x => x.InterviewerId)
                .Select(x => new {InterviewerId = x.Key, Traffic = x.Sum(s => (long?)s.Traffic) ?? 0})
                .ToList());

            return trafficUsage.ToDictionary(x => x.InterviewerId, x => x.Traffic);
        }

        public Dictionary<Guid, SyncStats> GetSynchronizationsStats(Guid[] interviewerIds)
        {
            var syncStat = this.dbContext.Query(devices =>
                (from device in devices
                    where interviewerIds.Contains(device.InterviewerId)
                    group device by device.InterviewerId
                    into grouping
                    select new
                    {
                        InterviewerId = grouping.Key,
                        FailedSyncCount = grouping.Count(x => x.Statistics == null),
                        SuccessfulSyncCount = grouping.Count(x => x.Statistics != null),
                        LastSync = grouping.Max(x=> x.SyncDate)
                    }).ToList());

            return syncStat.ToDictionary(x => x.InterviewerId, 
                x => new SyncStats(x.SuccessfulSyncCount, x.FailedSyncCount, x.LastSync));
        }

        public IEnumerable<DeviceSyncInfo> GetLastSyncByInterviewersList(Guid[] interviewerIds)
        {
            var syncWithNotEmptyStat = this.dbContext.Query(devices => 
                (from device in devices
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
                    DeviceInfoId = grouping.Max(x => x.Id)
                }).ToList());

            var lastSync = this.dbContext.Query(devices => (from device in devices
                where interviewerIds.Contains(device.InterviewerId)
                group device by device.InterviewerId
                into grouping
                select new
                {
                    grouping.Key,
                    DeviceInfoId = grouping.Max(x => x.Id)
                }).ToList());

            List<int> requiredDeviceInfos = new List<int>();
            foreach (var interviewerId in interviewerIds)
            {
                var notEmptyStat = syncWithNotEmptyStat.FirstOrDefault(x => x.Key == interviewerId);
                if (notEmptyStat != null)
                {
                    requiredDeviceInfos.Add(notEmptyStat.DeviceInfoId);
                    continue;
                }

                var lastStat = lastSync.FirstOrDefault(x => x.Key == interviewerId);
                if (lastStat!=null)
                    requiredDeviceInfos.Add(lastStat.DeviceInfoId);
            }

            return this.dbContext.Query(_ => _.Where(x => requiredDeviceInfos.Contains(x.Id)).ToList());
        }

        public int GetRegisteredDeviceCount(Guid interviewerId)
            => this.dbContext.Query(devices => devices
                .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId)
                .Select(info => info.DeviceId).Distinct().Count());

        public List<InterviewerDailyTrafficUsage> GetTrafficUsageForInterviewer(Guid interviewerId)
        {
            var dbData = this.dbContext.Query(devices =>
            {
                return devices
                    .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId)
                    .GroupBy(x => x.SyncDate.Date)
                    .Select(group => new
                    {
                        Key = group.Key,
                        DownloadBytes = group.Sum(s => (long?)s.Statistics.TotalDownloadedBytes),
                        UploadedBytes = group.Sum(s => (long?)s.Statistics.TotalUploadedBytes)
                    })
                    .OrderByDescending(x => x.Key)
                    .Take(30)
                    .ToList();
            });

            var list = dbData
                .OrderBy(x => x.Key)
                .Select(x => new InterviewerDailyTrafficUsage
                {
                    DownloadedBytes = x.DownloadBytes ?? 0,
                    UploadedBytes = x.UploadedBytes ?? 0,
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    Day = x.Key.Day
                }).ToList();

            return list;
        }

        public Task<long> GetTotalTrafficUsageForInterviewer(Guid interviewerId)
        {
            var totalTrafficUsed = this.dbContext.Query(devices => devices
                .Where(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.Statistics != null)
                .Select(x => new
                    {
                        TrafficUsed = x.Statistics.TotalUploadedBytes + x.Statistics.TotalDownloadedBytes
                    })
                .ToList());

            return Task.FromResult(totalTrafficUsed.Sum(x => x.TrafficUsed));
        }

        public double? GetAverageSynchronizationSpeedInBytesPerSeconds(Guid interviewerId)
        {
            var list = this.dbContext.Query(devices => devices.OrderByDescending(d => d.SyncDate)
                .Where(d => d.InterviewerId == interviewerId && d.Statistics != null)
                .Take(5)
                .ToList());

            return list.Count > 0 ? list.Average(info => info.Statistics.TotalConnectionSpeed) : (double?)null;
        }

        public Dictionary<Guid, double> GetAverageSynchronizationSpeedInBytesPerSeconds(Guid[] interviewerIds)
        {
            var syncWithEmptyStat = this.dbContext.Query(devices =>
                (from device in devices
                    where interviewerIds.Contains(device.InterviewerId)
                          && device.Statistics != null
                    group device by device.InterviewerId
                    into grouping
                    
                    select new
                    {
                        InterviewerId = grouping.Key,
                        AverageSpeed = grouping.Average(x => x.Statistics.TotalConnectionSpeed)
                    }).ToList());

            return syncWithEmptyStat.ToDictionary(x => x.InterviewerId, x => x.AverageSpeed);
        }

        public IEnumerable<DeviceSyncInfo> GetLastFailedByInterviewerIds(Guid[] interviewerIds)
        {
            var syncWithEmptyStat = this.dbContext.Query(devices =>
                (from device in devices
                    where interviewerIds.Contains(device.InterviewerId)
                          && device.Statistics == null
                    group device by device.InterviewerId
                    into grouping
                    select new
                    {
                        grouping.Key,
                        DeviceInfoId = grouping.Max(x => x.Id)
                    }).ToList());

            List<int> requiredDeviceInfos = syncWithEmptyStat.Select(x => x.DeviceInfoId).ToList();

            return this.dbContext.Query(_ => _.Where(x => requiredDeviceInfos.Contains(x.Id)).ToList());
        }

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId)
            => this.dbContext.Query(devices => devices.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.Statistics == null));

        public int GetSuccessSynchronizationsCount(Guid interviewerId)
            => this.dbContext.Query(devices => devices.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo != null));

        public int GetFailedSynchronizationsCount(Guid interviewerId)
            => this.dbContext.Query(devices => devices.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.Statistics == null));
        
        public SynchronizationActivity GetSynchronizationActivity(Guid interviewerId)
        {
            var toDay = DateTime.UtcNow;
            var fromDay = toDay.AddDays(-6);

            var deviceInfoByPeriod = this.dbContext.Query(devices => devices
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
                .ToList());

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
