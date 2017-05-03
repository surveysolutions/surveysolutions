using System;
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

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId != null);

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

        public SynchronizationActivity GetSynchronizationActivity(Guid id)
        {
            return new SynchronizationActivity
            {
                Days = new []
                {
                    new SyncDay
                    {
                        Day = new DateTime(2017, 01, 01),
                        Quarters = new []
                        {
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 1,
                                DownloadedQuestionnairesCount = 1,
                                StartedInterviewsCount = 3,
                                UploadedInterviewsCount = 0,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 2
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 0,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 0,
                                FailedSynchronizationsCount = 2,
                                SynchronizationsWithoutChangesCount = 0
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 0,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 0,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 3
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 2,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 0,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 0
                            },
                        }
                    },
                    new SyncDay
                    {
                        Day = new DateTime(2017, 01, 03),
                        Quarters = new []
                        {
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 0,
                                DownloadedQuestionnairesCount = 3,
                                StartedInterviewsCount = 8,
                                UploadedInterviewsCount = 2,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 1
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 5,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 2,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 1
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 0,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 0,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 1
                            },
                            new SyncDayQuarter
                            {
                                DownloadedInterviewsCount = 0,
                                DownloadedQuestionnairesCount = 0,
                                StartedInterviewsCount = 0,
                                UploadedInterviewsCount = 3,
                                FailedSynchronizationsCount = 0,
                                SynchronizationsWithoutChangesCount = 1
                            },
                        }
                    },
                }
            };
        }
    }
}