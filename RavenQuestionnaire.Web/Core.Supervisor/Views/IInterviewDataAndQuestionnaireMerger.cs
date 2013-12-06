using Core.Supervisor.Views.Interview;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views
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