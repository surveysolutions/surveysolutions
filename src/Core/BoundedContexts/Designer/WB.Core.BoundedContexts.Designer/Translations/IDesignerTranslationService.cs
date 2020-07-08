using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface IDesignerTranslationService : ITranslationsService
    {
        int Count(Guid questionnaireId, Guid translationId);
        void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId);
        TranslationFile GetTemplateAsExcelFile(Guid questionnaireId);
        bool HasTranslatedTitle(QuestionnaireDocument questionnaire);
    }
}
