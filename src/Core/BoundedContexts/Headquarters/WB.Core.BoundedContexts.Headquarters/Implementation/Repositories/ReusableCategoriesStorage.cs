using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class ReusableCategoriesStorage : IReusableCategoriesStorage
    {
        private readonly IPlainStorageAccessor<ReusableCategoricalOptions> storageAccessor;

        public ReusableCategoriesStorage(IPlainStorageAccessor<ReusableCategoricalOptions> storageAccessor)
        {
            this.storageAccessor = storageAccessor;
        }

        public IEnumerable<CategoriesItem> GetOptions(QuestionnaireIdentity questionnaireIdentity, Guid categoriesId)
        {
            var categoricalOptions = this.storageAccessor
                .Query(t =>
                    t.Where(categoricalOption => categoricalOption.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                                           && categoricalOption.QuestionnaireId.Version == questionnaireIdentity.Version
                                           && categoricalOption.CategoriesId == categoriesId)
                    .OrderBy(o => o.SortIndex)
                    .Select(co => new CategoriesItem()
                        {
                            ParentId = co.ParentValue,
                            Text = co.Text,
                            Id = co.Value
                        })
                    .ToList()
                );

            return categoricalOptions;
        }

        public void Store(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoriesItem> reusableCategories)
        {
            var enumerable = reusableCategories.Select((option, index) => new ReusableCategoricalOptions()
            {
                CategoriesId = categoryId,
                QuestionnaireId = questionnaireIdentity,
                SortIndex = index,
                ParentValue = option.ParentId,
                Text = option.Text,
                Value = option.Id
            }).Select(x => Tuple.Create(x, (object)x));

            this.storageAccessor.Store(enumerable);
        }
    }
}
