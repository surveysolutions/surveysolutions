using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface ISurveysAndStatusesReport
    {
        SurveysAndStatusesReportView Load(SurveysAndStatusesReportInputModel input);
    }
}