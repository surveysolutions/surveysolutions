using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, string culture);
        TranslationFile GetAsExcelFile(Guid questionnaireId, Guid? culture);
        void Store(Guid questionnaireId, string culture, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, string language, Guid newQuestionnaireId, string newCulture);
        void Delete(Guid questionnaireId, string culture);
    }
}