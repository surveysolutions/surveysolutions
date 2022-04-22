using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface ITranslationImportExportService
    {
        string GetTranslationsJson(QuestionnaireDocument questionnaire, Guid translationId);
        void StoreTranslationsFromJson(QuestionnaireDocument? questionnaire, Guid? translationId, string json);
    }
}
