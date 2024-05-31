using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Infrastructure.Native.Questionnaire.Impl
{
    public class ReusableCategoriesFillerIntoQuestionnaire : IReusableCategoriesFillerIntoQuestionnaire
    {
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;

        public ReusableCategoriesFillerIntoQuestionnaire(IReusableCategoriesStorage reusableCategoriesStorage)
        {
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        public QuestionnaireDocument FillCategoriesIntoQuestionnaireDocument(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument originalDocument)
        {
            var filledDocument = originalDocument.Clone();
            
            if (filledDocument.Categories.Any())
            {
                var categoriesCache = new ConcurrentDictionary<Guid, List<Answer>>();

                foreach (var question in filledDocument.Find<ICategoricalQuestion>())
                {
                    if (question.CategoriesId.HasValue)
                    {
                        var answers = categoriesCache.GetOrAdd(question.CategoriesId.Value,
                            key =>
                            {
                                var options = reusableCategoriesStorage.GetOptions(questionnaireIdentity,
                                    question.CategoriesId.Value);
                                    
                                    return options.Select(option => new Answer()
                                {
                                    AnswerCode = option.Id,
                                    AnswerText = option.Text,
                                    ParentCode = option.ParentId,
                                    ParentValue = option.ParentId?.ToString(CultureInfo.InvariantCulture),
                                    AnswerValue = option.Id.ToString(CultureInfo.InvariantCulture),
                                    AttachmentName = option.AttachmentName
                                }).ToList();
                            });

                        question.Answers = answers;
                    }
                }
            }

            return filledDocument;
        }
    }
}
