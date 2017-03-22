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

        public DeviceSyncInfo GetLastByInterviewerId(Guid enumeratorId)
            => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == enumeratorId);

        public DeviceSyncInfo GetLastSuccessByInterviewerId(Guid enumeratorId)
            => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == enumeratorId && deviceInfo.StatisticsId != null);

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid enumeratorId)
        => new HQPlainStorageDbContext().DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == enumeratorId && deviceInfo.StatisticsId == null);
    }
}