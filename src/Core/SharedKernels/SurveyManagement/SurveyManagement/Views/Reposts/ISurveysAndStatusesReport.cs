using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts
{
    public interface ISurveysAndStatusesReport {
        SurveysAndStatusesReportView Load(SurveysAndStatusesReportInputModel input);
    }
}