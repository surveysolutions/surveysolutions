using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface IQuestionnaireTranslator
    {
        QuestionnaireDocument Translate(QuestionnaireDocument originalDocument, ITranslation translation, bool useNullForEmptyTranslations = false);

        IComposite TranslateEntity(IComposite entity, ITranslation translation, bool useNullForEmptyTranslations = false);
    }
}
