using WB.Core.BoundedContexts.Headquarters.Views.SupervisorFeed;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ISupervisorFeedService
    {
        SupervisorRegisteredEntry GetEntry(string login);
    }
}