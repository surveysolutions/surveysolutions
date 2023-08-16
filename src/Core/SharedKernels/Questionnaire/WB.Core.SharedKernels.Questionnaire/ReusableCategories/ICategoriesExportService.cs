using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.SharedKernels.Questionnaire.ReusableCategories
{
    public interface ICategoriesExportService
    {
        byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items, bool isCascading, bool hqImport);
    }
}
