using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.Questionnaire.Categories
{
    public interface ICategories
    {
        List<CategoriesItem> GetCategories(Guid questionnaireId, Guid categoriesId);
    }
}
