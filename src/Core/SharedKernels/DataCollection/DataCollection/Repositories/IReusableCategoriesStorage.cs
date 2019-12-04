using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IReusableCategoriesStorage
    {
        IEnumerable<CategoriesItem> GetOptions(QuestionnaireIdentity identity, Guid categoriesId);
        void Store(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoriesItem> reusableCategories);
        void RemoveCategories(QuestionnaireIdentity questionnaireIdentity, Guid categoriesId);
    }
}
