using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Supervisor.Services
{
    public interface IQuestionnaireCacheInitializer
    {
        void InitializeQuestionnaireDocumentWithCaches(QuestionnaireDocument document);
    }
}