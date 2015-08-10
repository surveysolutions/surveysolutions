using Main.Core.Documents;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IQuestionnaireImportService
    {
        void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, string supportingAssembly, long version);
        void ImportQuestionnaireModel(QuestionnaireDocument questionnaireDocument, long version);
    }
}