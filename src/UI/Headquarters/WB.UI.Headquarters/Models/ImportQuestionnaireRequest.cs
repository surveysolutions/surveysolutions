using WB.Core.BoundedContexts.Headquarters.Views.Template;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ImportQuestionnaireRequest
    {
        public DesignerQuestionnaireListViewItem Questionnaire { get; set; }
        public bool AllowCensusMode { get; set; }
    }
}