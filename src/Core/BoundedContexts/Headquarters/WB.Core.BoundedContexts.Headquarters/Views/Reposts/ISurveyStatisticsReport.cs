using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface ISurveyStatisticsReport
    {
        ReportView GetReport(IQuestionnaire questionnaire, SurveyStatisticsReportInputModel perTeamReportInputModel);
    }
}
