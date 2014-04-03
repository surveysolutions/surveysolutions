using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Factories
{
    public interface IReferenceInfoForLinkedQuestionsFactory
    {
        ReferenceInfoForLinkedQuestions CreateReferenceInfoForLinkedQuestions(QuestionnaireDocument questionnaire, long version);
    }
}
