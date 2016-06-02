using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IReferenceInfoForLinkedQuestionsFactory
    {
        ReferenceInfoForLinkedQuestions CreateReferenceInfoForLinkedQuestions(QuestionnaireDocument questionnaire, long version);
    }
}
