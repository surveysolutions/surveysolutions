using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Infrastructure.Native.Questionnaire.Impl
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
                            Id = co.Value,
                            AttachmentName = co.AttachmentName
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
                Value = option.Id,
                AttachmentName = option.AttachmentName,
            }).Select(x => Tuple.Create(x, (object)x));

            this.storageAccessor.Store(enumerable);
        }

        public void RemoveCategories(QuestionnaireIdentity questionnaireIdentity)
        {
            this.storageAccessor.Remove(t =>
                t.Where(categoricalOption => categoricalOption.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                                             && categoricalOption.QuestionnaireId.Version == questionnaireIdentity.Version)
            );
        }

        public void Clone(QuestionnaireIdentity oldIdentity, QuestionnaireIdentity newIdentity)
        {
            var oldCategories = this.storageAccessor.Query(t => t.Where(categoricalOption =>
                    categoricalOption.QuestionnaireId.QuestionnaireId == oldIdentity.QuestionnaireId &&
                    categoricalOption.QuestionnaireId.Version == oldIdentity.Version))
                .ToList();

            var newCategories = oldCategories.Select(x => new ReusableCategoricalOptions()
            {
                CategoriesId = x.CategoriesId,
                QuestionnaireId = newIdentity,
                SortIndex = x.SortIndex,
                ParentValue = x.ParentValue,
                Text = x.Text,
                Value = x.Value,
                AttachmentName = x.AttachmentName,
            }).ToList();

            this.storageAccessor.Store(newCategories);
        }
    }
}
