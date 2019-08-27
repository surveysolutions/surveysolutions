using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISystemLogViewFactory
    {
        SystemLog GetLog(SystemLogFilter filter);
       
    }
}
