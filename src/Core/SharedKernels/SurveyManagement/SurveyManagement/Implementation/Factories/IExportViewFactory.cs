using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    public interface IExportViewFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version);
        InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure, InterviewData interview);
    }
}
