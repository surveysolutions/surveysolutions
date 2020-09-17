using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IDeviceSyncInfoRepository
    {
        void AddOrUpdate(DeviceSyncInfo deviceSyncInfo);
        DeviceSyncInfo GetLastByInterviewerId(Guid interviewerId);
        DateTime? GetLastSynchronizationDate(Guid interviewerId);
        DeviceSyncInfo GetLastSuccessByInterviewerId(Guid interviewerId);
        DeviceSyncInfo GetLastFailedByInterviewerId(Guid interviewerId);
        int GetSuccessSynchronizationsCount(Guid interviewerId);
        int GetFailedSynchronizationsCount(Guid interviewerId);
        double? GetAverageSynchronizationSpeedInBytesPerSeconds(Guid interviewerId);
        SynchronizationActivity GetSynchronizationActivity(Guid interviewerId);

        IEnumerable<DeviceSyncInfo> GetLastSyncByInterviewersList(Guid[] interviewerIds);

        int GetRegisteredDeviceCount(Guid interviewerId);
        List<InterviewerDailyTrafficUsage> GetTrafficUsageForInterviewer(Guid interviewerId);
        Task<long> GetTotalTrafficUsageForInterviewer(Guid interviewerId);
        Dictionary<Guid, long> GetInterviewersTrafficUsage(Guid[] interviewersIds);
        IEnumerable<DeviceSyncInfo> GetLastFailedByInterviewerIds(Guid[] interviewerIds);

        Dictionary<Guid, SyncStats> GetSynchronizationsStats(Guid[] interviewerIds);
        Dictionary<Guid, double> GetAverageSynchronizationSpeedInBytesPerSeconds(Guid[] interviewerIds);
    }
}
