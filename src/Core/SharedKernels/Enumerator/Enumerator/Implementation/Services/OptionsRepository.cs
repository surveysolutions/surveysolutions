using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
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

            var categoricalQuestionOptions = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString && 
                            x.QuestionId == questionId)
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
            var translationIdAsString = translationId.FormatGuid();

            filter = filter ?? String.Empty;
            var parentValueAsDecimal = parentValue.HasValue ? Convert.ToDecimal(parentValue) : (decimal?) null;

            int batchsize = 15;
            int lastLoadedSortIndex = -1;

            if (translationIdAsString == null)
            {
                List<OptionView> optionViews;

                do
                {
                    optionViews = this.optionsStorage
                        .FixedQuery(x => x.QuestionnaireId == questionnaireIdAsString &&
                                    x.QuestionId == questionId &&
                                    x.ParentValue == parentValueAsDecimal &&
                                    x.Title.ToLower().Contains(filter.ToLower()) &&
                                    x.SortOrder > lastLoadedSortIndex &&
                                    x.TranslationId == null,
                               y => y.SortOrder,
                               batchsize)
                        .ToList();

                    foreach (var option in optionViews)
                    {
                        yield return new CategoricalOption
                        {
                            ParentValue = option.ParentValue.HasValue ? Convert.ToInt32(option.ParentValue) : (int?) null,
                            Value = Convert.ToInt32(option.Value),
                            Title = option.Title
                        };
                    }
                    lastLoadedSortIndex = optionViews.LastOrDefault()?.SortOrder ?? 0;

                } while (optionViews.Any());
            }

            else
            {
                List<OptionView> mixedOptions;
                
                do
                {
                    mixedOptions = this.optionsStorage
                            .FixedQuery(x => x.QuestionnaireId == questionnaireIdAsString &&
                                             x.QuestionId == questionId &&
                                             x.ParentValue == parentValueAsDecimal &&
                                             x.Title.ToLower().Contains(filter.ToLower()) &&
                                             x.SortOrder > lastLoadedSortIndex &&
                                             (x.TranslationId == translationIdAsString || x.TranslationId == null),
                                        y => y.SortOrder,
                                        batchsize)
                             .ToList();
                    
                    lastLoadedSortIndex = mixedOptions.LastOrDefault()?.SortOrder ?? 0;

                    var defaultOptionValuesToCheck = mixedOptions.Where(x => x.TranslationId == null).Select(y => y.Value).ToList();

                    var translatedOptions = this.optionsStorage
                            .FixedQuery(x => x.QuestionnaireId == questionnaireIdAsString &&
                                             x.QuestionId == questionId &&
                                             x.ParentValue == parentValueAsDecimal &&
                                             defaultOptionValuesToCheck.Contains(x.Value) &&
                                             x.TranslationId == translationIdAsString ,
                                        y => y.SortOrder,
                                        batchsize)
                             .Select(y => y.Value)
                             .ToList();

                    foreach (var option in mixedOptions.Where(x => x.TranslationId != null || !translatedOptions.Contains(x.Value)))
                    {
                        yield return new CategoricalOption
                        {
                            ParentValue = option.ParentValue.HasValue ? Convert.ToInt32(option.ParentValue) : (int?)null,
                            Value = Convert.ToInt32(option.Value),
                            Title = option.Title
                        };
                    }

                } while (mixedOptions.Any());
            }
        }

        public CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionTitle, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption = null;

            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionId &&
                            x.Title == optionTitle &&
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

        public CategoricalOption GetQuestionOptionByValue(QuestionnaireIdentity questionnaireId, Guid questionId, decimal optionValue, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption;

            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionId &&
                            x.Value == optionValue &&
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

        public async Task StoreOptionsForQuestionAsync(QuestionnaireIdentity questionnaireIdentity, 
            Guid questionId, 
            List<Answer> answers, 
            List<TranslationDto> translations)
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
                    QuestionId = questionId,
                    Value = value,
                    ParentValue = parentValue,
                    Title = answer.AnswerText,
                    SortOrder = index,
                    TranslationId = null
                };

                optionsToSave.Add(optionView);

                var translatedOptions = translations.Where(x => x.QuestionnaireEntityId == questionId && 
                                                                x.TranslationIndex == answer.AnswerValue &&
                                                                x.Type == TranslationType.OptionTitle)
                    .Select(y => new OptionView
                    {
                        Id = $"{questionnaireIdAsString}-{questionIdAsString}-{answer.AnswerValue}-{y.TranslationId.FormatGuid()}",
                        QuestionnaireId = questionnaireIdAsString,
                        QuestionId = questionId,
                        Value = value,
                        ParentValue = parentValue,
                        Title = y.Value,
                        SortOrder = ++index,
                        TranslationId = y.TranslationId.FormatGuid()
                    }).ToList();

                optionsToSave.AddRange(translatedOptions);

                index++;
            }

            await this.optionsStorage.StoreAsync(optionsToSave);
        }
    }
}