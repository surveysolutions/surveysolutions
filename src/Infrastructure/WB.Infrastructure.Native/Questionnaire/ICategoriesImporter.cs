using System.Collections.Generic;
using System.IO;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Infrastructure.Native.Questionnaire
{
    public interface ICategoriesImporter
    {
        List<CategoriesItem> ExtractCategoriesFromExcelFile(Stream xmlFile);
    }
}
