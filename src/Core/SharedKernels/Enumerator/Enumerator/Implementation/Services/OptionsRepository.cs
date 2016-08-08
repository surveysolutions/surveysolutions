using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ninject.Selection;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class OptionsRepository : IOptionsRepository
    {
        private readonly IAsyncPlainStorage<OptionView> optionsStorage;
        
        public OptionsRepository(IAsyncPlainStorage<OptionView> optionsStorage)
        {
            this.optionsStorage = optionsStorage;
        }
        [Obsolete("Since V 5.10")]
        public IReadOnlyList<CategoricalOption> GetQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();

            var categoricalQuestionOptions = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString && 
                            x.QuestionId == questionIdAsString)
                .Select(x => new CategoricalOption
                {
                    ParentValue = x.ParentValue.HasValue ? Convert.ToInt32(x.ParentValue) : (int?)null,
                    Value = Convert.ToInt32(x.Value),
                    Title = x.Title
                })
                .OrderBy(x => x.Title)
                .ToReadOnlyCollection();

            return categoricalQuestionOptions;
        }

        public IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId, 
            int? parentValue, string filter, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            filter = filter ?? String.Empty;
            var parentValueAsDecimal = parentValue.HasValue ? Convert.ToDecimal(parentValue) : (decimal?) null;

            var optionViews = this.optionsStorage
                    .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                                x.QuestionId == questionIdAsString &&
                                x.ParentValue == parentValueAsDecimal &&
                                x.Title.ToLower().Contains(filter.ToLower()) &&
                                (x.TranslationId == translationIdAsString || x.TranslationId == null))
                    .GroupBy(y => new { y.Value , y.QuestionnaireId , y.QuestionId , y.ParentValue})
                    .Select(group => group.OrderByDescending(x => x.TranslationId == translationIdAsString).First());

            var loadedBatch = optionViews.Select(x => new CategoricalOption
            {
                ParentValue = x.ParentValue.HasValue ? Convert.ToInt32(x.ParentValue) : (int?) null,
                Value = Convert.ToInt32(x.Value),
                Title = x.Title
            }).ToList();

            foreach (var option in loadedBatch)
            {
                yield return option;
            }
        }

        public CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionValue, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption = null;

            
            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionIdAsString &&
                            x.Title == optionValue &&
                            (x.TranslationId == translationIdAsString || x.TranslationId == null))
                .OrderBy(x => x.TranslationId != null)
                .FirstOrDefault();

            if (categoricalQuestionOption == null)
                return null;

            return new CategoricalOption
            {
                ParentValue = categoricalQuestionOption.ParentValue.HasValue ? Convert.ToInt32(categoricalQuestionOption.ParentValue) : (int?)null,
                Value = Convert.ToInt32(categoricalQuestionOption.Value),
                Title = categoricalQuestionOption.Title
            };
        }

        public async Task RemoveOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var optionsToDelete = this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString).ToList();
            await this.optionsStorage.RemoveAsync(optionsToDelete);
        }
        
        public bool IsEmpty() => this.optionsStorage.FirstOrDefault() == null;

        public async Task StoreOptionsForQuestionAsync(QuestionnaireIdentity questionnaireIdentity, Guid questionId, List<Answer> answers, List<TranslationDto> translations)
        {
            var questionIdAsString = questionId.FormatGuid();
            var questionnaireIdAsString = questionnaireIdentity.ToString();

            var optionsToSave = new List<OptionView>();

            int index = 0;

            foreach (var answer in answers)
            {
                decimal value = answer.AnswerCode ?? decimal.Parse(answer.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture);
                decimal? parentValue = null;
                if (!string.IsNullOrEmpty(answer.ParentValue))
                {
                    parentValue = decimal.Parse(answer.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture);
                }
                var id = $"{questionnaireIdAsString}-{questionIdAsString}-{answer.AnswerValue}";

                var optionView = new OptionView
                {
                    Id = id,
                    QuestionnaireId = questionnaireIdAsString,
                    QuestionId = questionIdAsString,
                    Value = value,
                    ParentValue = parentValue,
                    Title = answer.AnswerText,
                    SortOrder = index,
                    TranslationId = null
                };

                optionsToSave.Add(optionView);

                var translatedOptions = translations.Where(x => x.QuestionnaireEntityId == questionId && x.TranslationIndex == answer.AnswerValue)
                    .Select(y => new OptionView
                    {
                        Id = $"{questionnaireIdAsString}-{questionIdAsString}-{answer.AnswerValue}-{y.TranslationId.FormatGuid()}",
                        QuestionnaireId = questionnaireIdAsString,
                        QuestionId = questionIdAsString,
                        Value = value,
                        ParentValue = parentValue,
                        Title = y.Value,
                        SortOrder = index++,
                        TranslationId = y.TranslationId.FormatGuid()
                    }).ToList();

                optionsToSave.AddRange(translatedOptions);

                index++;
            }

            await this.optionsStorage.StoreAsync(optionsToSave);
        }

        public async Task StoreOptionsAsync(List<OptionView> options)
        {
            await this.optionsStorage.StoreAsync(options);
        }
        
    }
}