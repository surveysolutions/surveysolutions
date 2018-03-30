using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IDeviceSyncInfoRepository
    {
        void AddOrUpdate(DeviceSyncInfo deviceSyncInfo);
        DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId);
        DateTime? GetLastSyncronizationDate(Guid interviewerId);
        DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId);
        DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId);
        int GetSuccessSynchronizationsCount(Guid interviewerId);
        int GetFailedSynchronizationsCount(Guid interviewerId);
        double? GetAverageSynchronizationSpeedInBytesPerSeconds(Guid interviewerId);
        SynchronizationActivity GetSynchronizationActivity(Guid interviewerId);

        IEnumerable<DeviceSyncInfo> GetLastSyncByInterviewersList(Guid[] interviewerIds);

        int GetRegistredDeviceCount(Guid interviewerId);
    }
}
