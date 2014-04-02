using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQuestionnaireCacheInitializer
    {
        void InitializeQuestionnaireDocumentWithCaches(QuestionnaireDocument document);
    }
}