using System;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IDeviceSyncInfoRepository
    {
        void AddOrUpdate(DeviceSyncInfo deviceSyncInfo);
        DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId);
        DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId);
        DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId);
    }
}