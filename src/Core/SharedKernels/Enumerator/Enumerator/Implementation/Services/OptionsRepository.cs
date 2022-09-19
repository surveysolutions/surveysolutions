using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class OptionsRepository : IOptionsRepository
    {
        private readonly IPlainStorage<OptionView, int?> optionsStorage;
        
        public OptionsRepository(IPlainStorage<OptionView, int?> optionsStorage)
        {
            this.optionsStorage = optionsStorage;
        }

        [DebuggerDisplay("{Value} {TranslationId == null ? Title : Title +\"(translation)\"}")]
        private class Option: OptionValue
        {
            public string Title { get; set; }

            public decimal? ParentValue { get; set; }

            public string TranslationId { get; set; }

            public string AttachmentName { get; set; }
        }

        [DebuggerDisplay("{Value}")]
        private class OptionValue
        {
            public decimal Value { get; set; }
        }

        public IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId,
            Guid questionId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null)
        {
            return GetFilteredCategoriesOptionsImpl(questionnaireId, questionId, null, parentValue, filter, translationId, excludedOptionIds);
        }

        public IEnumerable<CategoricalOption> GetFilteredCategoriesOptions(QuestionnaireIdentity questionnaireId,
            Guid categoryId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null)
        {
            return GetFilteredCategoriesOptionsImpl(questionnaireId, null, categoryId, parentValue, filter, translationId, excludedOptionIds);
        }

        private IEnumerable<CategoricalOption> GetFilteredCategoriesOptionsImpl(QuestionnaireIdentity questionnaireId,
            Guid? questionId, Guid? categoryId, int? parentValue, string filter, Guid? translationId, int[] excludedOptionIds = null)
        {
            if (!categoryId.HasValue && !questionId.HasValue)
                throw new ArgumentException("Should specify questionId or categoryId");

            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var categoryIdAsString = categoryId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            filter = (filter ?? String.Empty).ToLower();
            decimal? parentValueAsDecimal = parentValue;
            decimal[] excludedOptionIdsAsDecimal = excludedOptionIds?.Select(x => (decimal) x)?.ToArray() ?? Array.Empty<decimal>();

            int take = 50;
            int skip = 0;

            List<Option> optionViews;

            do
            {
                optionViews = this.optionsStorage
                    .FixedQueryWithSelection(
                        @where => @where.QuestionnaireId == questionnaireIdAsString &&
                                  @where.QuestionId == questionIdAsString && 
                                  @where.CategoryId == categoryIdAsString &&
                                  (parentValueAsDecimal == null || @where.ParentValue == parentValueAsDecimal) &&
                                  (filter == "" || filter == null || @where.SearchTitle.Contains(filter)) &&
                                  (@where.TranslationId == translationIdAsString || @where.TranslationId == null) &&
                                  !excludedOptionIdsAsDecimal.Contains(@where.Value),
                        @order => @order.SortOrder,
                        @select => new Option
                        {
                            Value = @select.Value,
                            Title = @select.Title,
                            ParentValue = @select.ParentValue,
                            TranslationId = @select.TranslationId,
                            AttachmentName = @select.AttachmentName,
                        },
                        take, skip)
                    .ToList();

                if (translationIdAsString != null)
                {
                    var defaultOptionValuesToCheck = optionViews.Where(x => x.TranslationId == null)
                        .Select(y => y.Value).ToList();

                    var translatedOptions = this.optionsStorage
                        .FixedQueryWithSelection(
                            @where => @where.QuestionnaireId == questionnaireIdAsString &&
                                      @where.QuestionId == questionIdAsString &&
                                      @where.CategoryId == categoryIdAsString &&
                                      (parentValueAsDecimal == null || @where.ParentValue == parentValueAsDecimal) &&
                                      defaultOptionValuesToCheck.Contains(@where.Value) &&
                                      @where.TranslationId == translationIdAsString,
                            @order => @order.SortOrder,
                            @select => new OptionValue {Value = @select.Value},
                            take)
                        .Select(x => x.Value)
                        .ToList();

                    optionViews = optionViews
                        .Where(x => x.TranslationId != null || !translatedOptions.Contains(x.Value)).ToList();
                }

                foreach (var option in optionViews)
                {
                    yield return new CategoricalOption
                    {
                        ParentValue = option.ParentValue.HasValue ? Convert.ToInt32(option.ParentValue) : (int?) null,
                        Value = Convert.ToInt32(option.Value),
                        Title = option.Title,
                        AttachmentName = option.AttachmentName,
                    };
                }

                skip += take;
                // increasing batch size on each query iteration, but no more then 1000
                take = Math.Min(skip * 2, 1000);
            } while (optionViews.Any());
        }

        public CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionTitle, int? parentQuestionValue, Guid? translationId)
        {
            return GetOptionImpl(questionnaireId, questionId, null, optionTitle, parentQuestionValue, translationId);
        }

        public CategoricalOption GetCategoryOption(QuestionnaireIdentity questionnaireId, Guid categoryId, string optionTitle,
            int? parentQuestionValue, Guid? translationId)
        {
            return GetOptionImpl(questionnaireId, null, categoryId, optionTitle, parentQuestionValue, translationId);
        }

        private CategoricalOption GetOptionImpl(QuestionnaireIdentity questionnaireId, Guid? questionId, Guid? categoryId, string optionTitle,
            int? parentQuestionValue, Guid? translationId)
        {
            if (!categoryId.HasValue && !questionId.HasValue)
                throw new ArgumentException("Should specify questionId or categoryId");

            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId?.FormatGuid();
            var categoryIdAsString = categoryId?.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption = null;

            optionTitle = optionTitle.ToLower();

            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionIdAsString &&
                            x.CategoryId == categoryIdAsString &&
                            x.SearchTitle.ToLower() == optionTitle &&
                            (parentQuestionValue == null || x.ParentValue == parentQuestionValue) &&
                            (x.TranslationId == translationIdAsString || x.TranslationId == null))
                .OrderBy(x => x.TranslationId == null)
                .FirstOrDefault();

            if (categoricalQuestionOption == null)
                return null;

            return new CategoricalOption
            {
                ParentValue = categoricalQuestionOption.ParentValue.HasValue ? Convert.ToInt32(categoricalQuestionOption.ParentValue) : (int?)null,
                Value = Convert.ToInt32(categoricalQuestionOption.Value),
                Title = categoricalQuestionOption.Title,
                AttachmentName = categoricalQuestionOption.AttachmentName,
            };
        }

        public CategoricalOption GetQuestionOptionByValue(QuestionnaireIdentity questionnaireId, Guid questionId, decimal optionValue, int? parentValue, Guid? translationId)
        {
            return GetOptionByValue(questionnaireId, questionId, null, optionValue, parentValue, translationId);
        }

        public CategoricalOption GetCategoryOptionByValue(QuestionnaireIdentity questionnaireId, Guid categoryId,
            decimal optionValue, int? parentValue, Guid? translationId)
        {
            return GetOptionByValue(questionnaireId, null, categoryId, optionValue, parentValue, translationId);
        }

        private CategoricalOption GetOptionByValue(QuestionnaireIdentity questionnaireId, Guid? questionId, Guid? categoryId, decimal optionValue, int? parentValue, Guid? translationId)
        {
            if (!categoryId.HasValue && !questionId.HasValue)
                throw new ArgumentException("Should specify questionId or categoryId");

            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var categoryIdAsString = categoryId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            var categoricalQuestionOption = GetOptionByValue(optionValue, parentValue, questionnaireIdAsString, questionIdAsString, categoryIdAsString, translationIdAsString);

            if (categoricalQuestionOption == null)
                return null;

            return new CategoricalOption
            {
                ParentValue = categoricalQuestionOption.ParentValue.HasValue ? Convert.ToInt32(categoricalQuestionOption.ParentValue) : (int?)null,
                Value = Convert.ToInt32(categoricalQuestionOption.Value),
                Title = categoricalQuestionOption.Title,
                AttachmentName = categoricalQuestionOption.AttachmentName,
            };
        }

        private OptionView GetOptionByValue(decimal optionValue, int? parentValue, string questionnaireIdAsString,
            string questionIdAsString, string categoryIdAsString, string translationIdAsString) =>
            this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                                           x.QuestionId == questionIdAsString &&
                                           x.CategoryId == categoryIdAsString &&
                                           x.Value == optionValue &&
                                           (parentValue == null || x.ParentValue == parentValue) &&
                                           (x.TranslationId == translationIdAsString || x.TranslationId == null))
                .OrderBy(x => x.TranslationId == null)
                .FirstOrDefault();

        public CategoricalOption[] GetOptionsByValues(QuestionnaireIdentity questionnaireId, Guid questionId, int[] optionValues, Guid? translationId)
        {
            return GetOptionsByValuesImpl(questionnaireId, questionId, null, optionValues, translationId);
        }

        public CategoricalOption[] GetCategoryOptionsByValues(QuestionnaireIdentity questionnaireIdentity, Guid categoryId,
            int[] optionsValues, Guid? translationId)
        {
            return GetOptionsByValuesImpl(questionnaireIdentity, null, categoryId, optionsValues, translationId);
        }

        private CategoricalOption[] GetOptionsByValuesImpl(QuestionnaireIdentity questionnaireId, Guid? questionId, Guid? categoryId, int[] optionValues, Guid? translationId)
        {
            if (!categoryId.HasValue && !questionId.HasValue)
                throw new ArgumentException("Should specify questionId or categoryId");

            if (optionValues.Length == 0) return Array.Empty<CategoricalOption>();

            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var categoryIdAsString = categoryId.FormatGuid();
            var values = optionValues.Select(Convert.ToDecimal).ToArray();
            var translationIdAsString = translationId.FormatGuid();

            List<CategoricalOption> result = new List<CategoricalOption>(optionValues.Length);

            for (int i = 0; i < values.Length; i++)
            {
                var option = GetOptionByValue(values[i], null, questionnaireIdAsString, questionIdAsString, categoryIdAsString,
                    translationIdAsString);
                result.Add(ToCategoricalOption(option));
            }

            return result.ToArray();
        }



        private CategoricalOption ToCategoricalOption(OptionView option) => new CategoricalOption
        {
            ParentValue = option.ParentValue.HasValue ? Convert.ToInt32(option.ParentValue) : (int?) null,
            Value = Convert.ToInt32(option.Value),
            Title = option.Title,
            AttachmentName = option.AttachmentName,
        };

        public void RemoveOptionsForQuestionnaire(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var optionsToDelete = this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString).ToList();
            this.optionsStorage.Remove(optionsToDelete);
        }

        public bool IsEmpty() => this.optionsStorage.FirstOrDefault() == null;

        public CategoricalOption[] GetReusableCategoriesById(QuestionnaireIdentity questionnaireId, Guid categoryId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var categoryIdAsString = categoryId.FormatGuid();

            var options = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == null &&
                            x.CategoryId == categoryIdAsString &&
                            x.TranslationId == null)
                .OrderBy(x => x.SortOrder);

            return options.Select(ToCategoricalOption).ToArray();
        }

        public void StoreOptionsForQuestion(QuestionnaireIdentity questionnaireIdentity, Guid questionId, List<Answer> answers, List<TranslationDto> translations)
        {
            StoreOptionsImpl(questionnaireIdentity, questionId, null, answers, translations);
        }

        public void StoreOptionsForCategory(QuestionnaireIdentity questionnaireIdentity, Guid categoryId, List<CategoriesItem> options, List<TranslationDto> translations)
        {
            var answers = options.Select(a => new Answer()
            {
                AnswerText = a.Text,
                ParentValue = a.ParentId.ToString(),
                AnswerValue = a.Id.ToString(),
                AttachmentName = a.AttachmentName,
            }).ToList();
            StoreOptionsImpl(questionnaireIdentity, null, categoryId, answers, translations);
        }


        private void StoreOptionsImpl(QuestionnaireIdentity questionnaireIdentity, Guid? questionId, 
            Guid? categoryId, List<Answer> options, List<TranslationDto> translations)
        {
            if (!categoryId.HasValue && !questionId.HasValue)
                throw new ArgumentException("Should specify questionId or categoryId");

            var questionIdAsString = questionId.FormatGuid();
            var categoryIdAsString = categoryId.FormatGuid();
            var questionnaireIdAsString = questionnaireIdentity.ToString();

            var optionsToSave = new List<OptionView>();

            int index = 0;

            foreach (var option in options)
            {
                decimal value = option.GetParsedValue();
                decimal? parentValue = null;
                if (!string.IsNullOrEmpty(option.ParentValue))
                {
                    parentValue = decimal.Parse(option.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture);
                }

                var optionView = new OptionView
                {
                    QuestionnaireId = questionnaireIdAsString,
                    QuestionId = questionIdAsString,
                    Value = value,
                    ParentValue = parentValue,
                    Title = option.AnswerText,
                    SearchTitle = option.AnswerText?.ToLower(),
                    SortOrder = index,
                    TranslationId = null,
                    CategoryId = categoryIdAsString,
                    AttachmentName = option.AttachmentName,
                };

                optionsToSave.Add(optionView);
                //now includes fallback to default value
                var translatedOptions =  translations.Where(x => 
                    ((x.QuestionnaireEntityId == questionId && x.Type == TranslationType.OptionTitle)
                    ||(x.QuestionnaireEntityId == categoryId && x.Type == TranslationType.Categories))
                      && (x.TranslationIndex == $"{option.AnswerValue}${option.ParentValue}" 
                          || x.TranslationIndex == option.AnswerValue))
                    .Select(y => new OptionView
                    {
                        QuestionnaireId = questionnaireIdAsString,
                        QuestionId = questionIdAsString,
                        Value = value,
                        ParentValue = parentValue,
                        Title = y.Value,
                        SearchTitle = y.Value?.ToLower(),
                        SortOrder = ++index,
                        TranslationId = y.TranslationId.FormatGuid(),
                        CategoryId = categoryIdAsString,
                        AttachmentName = optionView.AttachmentName,
                    }).ToList();

                optionsToSave.AddRange(translatedOptions);

                index++;
            }

            this.optionsStorage.Store(optionsToSave);
        }

        public void StoreOptions(List<OptionView> options)
        {
            this.optionsStorage.Store(options);
        }
    }
}
