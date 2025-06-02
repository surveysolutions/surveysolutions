using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface IDesignerTranslationService : ITranslationsService
    {
        int Count(Guid questionnaireId, Guid translationId);
        void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId);
        TranslationFile GetTemplateAsExcelFile(QuestionnaireRevision questionnaireId);
        TranslationFile GetAsExcelFile(QuestionnaireRevision questionnaireId, Guid translationId);
        bool HasTranslatedTitle(QuestionnaireDocument questionnaire);

        IEnumerable<TranslationInstance> GetFromQuestionnaire(QuestionnaireDocument questionnaire);
        bool IsFullTranslated(QuestionnaireDocument questionnaire, ITranslation translation);
        void Store(IEnumerable<TranslationInstance> translationInstances);
    }
}
