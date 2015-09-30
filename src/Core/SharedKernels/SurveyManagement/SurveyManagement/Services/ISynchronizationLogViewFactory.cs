using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface ISynchronizationLogViewFactory
    {
        SynchronizationLog GetLog(SynchronizationLogFilter filter);
        UsersView GetInterviewers(int pageSize, string searchBy);
        SynchronizationLogDevicesView GetDevices(int pageSize, string searchBy);
    }
}
