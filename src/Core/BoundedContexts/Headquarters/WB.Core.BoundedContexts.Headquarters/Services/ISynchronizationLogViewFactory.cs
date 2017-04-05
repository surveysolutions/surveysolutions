using System;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISynchronizationLogViewFactory
    {
        SynchronizationLog GetLog(SynchronizationLogFilter filter);
        UsersView GetInterviewers(int pageSize, string searchBy);
        SynchronizationLogDevicesView GetDevices(int pageSize, string searchBy);
        InterviewLog GetInterviewLog(Guid interviewId, Guid responsibleId);
    }
}
