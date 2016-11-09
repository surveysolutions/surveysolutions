using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, Guid translationId);
        TranslationFile GetAsExcelFile(Guid questionnaireId, Guid translationId);
        TranslationFile GetTemplateAsExcelFile(Guid questionnaireId);
        void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        int Count(Guid questionnaireId, Guid translationId);
    }
}