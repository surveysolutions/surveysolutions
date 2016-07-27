using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, Guid translationId);
        TranslationFile GetAsExcelFile(Guid questionnaireId, Guid translationId);
        TranslationFile GetAsOpenOfficeFile(Guid questionnaireId, Guid translationId);
        TranslationFile GetTemplateAsExcelFile(Guid questionnaireId);
        TranslationFile GetTemplateAsOpenOfficeFile(Guid questionnaireId);
        void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId);
        void Delete(Guid questionnaireId, Guid translationId);
        int Count(Guid questionnaireId, Guid translationId);
    }
}