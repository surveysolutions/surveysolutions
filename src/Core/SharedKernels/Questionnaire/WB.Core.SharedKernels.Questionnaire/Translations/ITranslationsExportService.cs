using System;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslationsExportService
    {
        TranslationFile GenerateTranslationFile(QuestionnaireDocument questionnaire, Guid translationId,
            ITranslation translation = null);
    }
}
