using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface ICategoriesImportExportService
    {
        string GetCategoriesJson(Guid questionnaireId, Guid categoriesId);
        void StoreCategoriesFromJson(Guid questionnaireId, Guid categoriesId, string json);
    }
}