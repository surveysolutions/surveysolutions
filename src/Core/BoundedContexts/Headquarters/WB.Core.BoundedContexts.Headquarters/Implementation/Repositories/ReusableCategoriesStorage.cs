using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
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

        public IEnumerable<CategoricalOption> GetOptions(QuestionnaireIdentity questionnaireIdentity, Guid categoriesId)
        {
            var categoricalOptions = this.storageAccessor
                .Query(t =>
                    t.Where(categoricalOption => categoricalOption.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                                           && categoricalOption.QuestionnaireId.Version == questionnaireIdentity.Version
                                           && categoricalOption.CategoriesId == categoriesId)
                    .OrderBy(o => o.Order)
                    .Select(co => new CategoricalOption()
                        {
                            ParentValue = co.ParentValue,
                            Title = co.Text,
                            Value = co.Value
                        })
                    .ToList()
                );

            return categoricalOptions;
        }

        public void Store(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoricalOption> reusableCategories)
        {
            var enumerable = reusableCategories.Select((option, index) => new ReusableCategoricalOptions()
            {
                CategoriesId = categoryId,
                QuestionnaireId = questionnaireIdentity,
                Order = index,
                ParentValue = option.ParentValue,
                Text = option.Title,
                Value = option.Value
            }).Select(x => Tuple.Create(x, (object)x));

            this.storageAccessor.Store(enumerable);
        }
    }
}
