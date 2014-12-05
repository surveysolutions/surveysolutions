using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public interface IInterviewDataAndQuestionnaireMerger
    {
        InterviewDetailsView Merge(InterviewData interview,
            QuestionnaireDocumentVersioned questionnaire,
            ReferenceInfoForLinkedQuestions questionnaireReferenceInfo,
            QuestionnaireRosterStructure questionnaireRosters,
            UserDocument user);
    }
}