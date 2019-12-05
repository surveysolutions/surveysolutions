using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.ReusableCategories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;

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

        public void RemoveCategories(QuestionnaireIdentity questionnaireIdentity, Guid categoriesId)
        {
            var items = this.storageAccessor
                .Query(t =>
                    t.Where(categoricalOption => categoricalOption.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                                                 && categoricalOption.QuestionnaireId.Version == questionnaireIdentity.Version
                                                 && categoricalOption.CategoriesId == categoriesId)
                ).ToList();

            this.storageAccessor.Remove(items);
        }

        public void FillCategoriesIntoQuestionnaireDocument(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
        {
            if (questionnaireDocument.Categories.Any())
            {
                foreach (var question in questionnaireDocument.Find<ICategoricalQuestion>())
                {
                    if (question.CategoriesId.HasValue)
                    {
                        var options = GetOptions(questionnaireIdentity, question.CategoriesId.Value);
                        question.Answers = options.Select(option => new Answer()
                        {
                            AnswerCode = option.Id,
                            AnswerText = option.Text,
                            ParentCode = option.ParentId,
                            ParentValue = option.ParentId?.ToString(),
                            AnswerValue = option.Id.ToString(),
                        }).ToList();
                    }
                }
            }
        }
    }
}
