using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.SharedKernels.SurveySolutions.ReusableCategories
{
    public interface IReusableCategoriesExporter
    {
        byte[] GetAsExcelFile(IEnumerable<CategoriesItem> items);
    }
}
