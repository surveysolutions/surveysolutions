using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface ISupervisorTeamsAndStatusesReport
    {
        TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input);
    }
}