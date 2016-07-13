using Main.Core.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface IQuestionnaireTranslator
    {
        QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, ITranslation translation);
    }
}