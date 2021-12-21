using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface ICategoriesImportExportService
    {
        string GetCategoriesJson(QuestionnaireDocument questionnaire, Guid categoriesId);
        void StoreCategoriesFromJson(QuestionnaireDocument questionnaire, Guid categoriesId, string json);
    }
}