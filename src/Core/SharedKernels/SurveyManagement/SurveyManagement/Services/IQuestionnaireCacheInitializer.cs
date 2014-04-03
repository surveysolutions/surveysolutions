using Main.Core.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IQuestionnaireCacheInitializer
    {
        void InitializeQuestionnaireDocumentWithCaches(QuestionnaireDocument document);
    }
}