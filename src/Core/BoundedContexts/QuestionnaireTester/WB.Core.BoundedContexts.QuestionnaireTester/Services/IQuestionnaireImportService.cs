using Main.Core.Documents;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    internal interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument);
    }
}