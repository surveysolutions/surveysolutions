using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IReusableCategoriesStorage
    {
        IEnumerable<CategoricalOption> GetOptions(QuestionnaireIdentity identity, Guid categoriesId);
        void Store(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoricalOption> reusableCategories);
    }
}
