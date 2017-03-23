using System;
using System.Data.Entity.Migrations;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DeviceSyncInfoRepository : IDeviceSyncInfoRepository
    {
        public void AddOrUpdate(DeviceSyncInfo deviceSyncInfo)
        {
            var dbContext = new HQPlainStorageDbContext();
            if (deviceSyncInfo.Statistics != null)
                deviceSyncInfo.StatisticsId = dbContext.SyncStatistics.Add(deviceSyncInfo.Statistics).Id;
            dbContext.DeviceSyncInfo.AddOrUpdate(deviceSyncInfo);
            dbContext.SaveChanges();
        }

        public DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId)
            => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId);

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId)
            => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId != null);

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId)
        => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);

        public int GetSuccessSynchronizationsCount(Guid interviewerId)
        => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);

        public int GetFailedSynchronizationsCount(Guid interviewerId)
        => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);
    }
}