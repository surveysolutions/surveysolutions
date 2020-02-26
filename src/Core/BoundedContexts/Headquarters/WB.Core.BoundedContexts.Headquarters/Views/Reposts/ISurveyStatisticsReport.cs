using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts
{
    public interface ISurveyStatisticsReport
    {
        ReportView GetReport(IQuestionnaire questionnaire, SurveyStatisticsReportInputModel perTeamReportInputModel);
    }
}
