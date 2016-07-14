using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, string language);
        TranslationFile GetAsExcelFile(Guid questionnaireId, Guid? languageId);
        void Store(Guid questionnaireId, string language, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, string language, Guid newQuestionnaireId, string newCulture);
        void Delete(Guid questionnaireId, string language);
    }
}