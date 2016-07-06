using Main.Core.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translator
{
    public interface IQuestionnaireTranslator
    {
        QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, IQuestionnaireTranslation translation);
    }
}