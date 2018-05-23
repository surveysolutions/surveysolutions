using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface ISurveyStatisticsReport
    {
        ReportView GetReport(SurveyStatisticsReportInputModel perTeamReportInputModel);
    }
}
