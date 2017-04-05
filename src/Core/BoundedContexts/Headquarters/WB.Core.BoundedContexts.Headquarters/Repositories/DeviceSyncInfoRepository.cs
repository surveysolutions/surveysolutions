﻿using System;
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

        public DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .FirstOrDefault(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);

        public int GetSuccessSynchronizationsCount(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId != null);

        public int GetFailedSynchronizationsCount(Guid interviewerId)
            => this.dbContext.DeviceSyncInfo.OrderByDescending(deviceInfo => deviceInfo.Id)
                .Count(deviceInfo => deviceInfo.InterviewerId == interviewerId && deviceInfo.StatisticsId == null);
    }
}