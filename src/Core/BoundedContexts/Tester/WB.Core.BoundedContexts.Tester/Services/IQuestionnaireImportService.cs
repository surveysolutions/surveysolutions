using Main.Core.Documents;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, string supportingAssembly);
    }
}