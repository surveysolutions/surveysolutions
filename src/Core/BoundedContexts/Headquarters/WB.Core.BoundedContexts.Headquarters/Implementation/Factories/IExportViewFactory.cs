using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public interface IExportViewFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version);
        InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure, InterviewData interview);
    }
}
