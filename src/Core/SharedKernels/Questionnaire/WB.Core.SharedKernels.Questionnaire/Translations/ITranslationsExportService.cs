using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.SharedKernels.Questionnaire.Translations
{
    public interface ITranslationsExportService
    {
        TranslationFile GenerateTranslationFile(QuestionnaireDocument questionnaire, Guid translationId,
            ITranslation translation, ICategories categories);

        Dictionary<string, string> GetExportedAliasesForTranslations(QuestionnaireDocument questionnaire,
            ITranslation translation, ICategories categoriesService);
    }
}
