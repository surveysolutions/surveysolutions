using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, string supportingAssembly);
    }
}