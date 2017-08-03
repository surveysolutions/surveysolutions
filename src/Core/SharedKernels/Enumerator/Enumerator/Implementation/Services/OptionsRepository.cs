using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly IPlainStorage<OptionView> optionsStorage;
        
        public OptionsRepository(IPlainStorage<OptionView> optionsStorage)
        {
            this.optionsStorage = optionsStorage;
        }

        private class Option: OptionValue
        {
            public string Title { get; set; }

            public decimal? ParentValue { get; set; }

            public string TranslationId { get; set; }
        }

        private class OptionValue
        {
            public decimal Value { get; set; }
        }

        public IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId,
            Guid questionId,
            int? parentValue, string filter, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            filter = (filter ?? String.Empty).ToLower();
            decimal? parentValueAsDecimal = parentValue ?? null;

            int take = 50;
            int skip = 0;

            List<Option> optionViews;

            do
            {
                optionViews = this.optionsStorage
                    .FixedQueryWithSelection(
                        @where => @where.QuestionnaireId == questionnaireIdAsString &&
                                  @where.QuestionId == questionIdAsString &&
                                  @where.ParentValue == parentValueAsDecimal &&
                                  (filter == "" || filter == null || @where.SearchTitle.Contains(filter)) &&
                                  (@where.TranslationId == translationIdAsString || @where.TranslationId == null),
                        @order => @order.SortOrder,
                        @select => new Option
                        {
                            Value = @select.Value,
                            Title = @select.Title,
                            ParentValue = @select.ParentValue,
                            TranslationId = @select.TranslationId
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
                                      @where.ParentValue == parentValueAsDecimal &&
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
                        Title = option.Title
                    };
                }

                skip += take;
                // increasing batch size on each query iteration, but no more then 1000
                take = Math.Min(skip * 2, 1000);
            } while (optionViews.Any());

        }

        public CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionTitle, int? parentQuestionValue, Guid? translationId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption = null;

            optionTitle = optionTitle.ToLower();

            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionIdAsString &&
                            x.SearchTitle.ToLower() == optionTitle &&
                            x.ParentValue == parentQuestionValue &&
                            (x.TranslationId == translationIdAsString || x.TranslationId == null))
                .OrderBy(x => x.TranslationId == null)
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
            var questionIdAsString = questionId.FormatGuid();
            var translationIdAsString = translationId.FormatGuid();

            OptionView categoricalQuestionOption = null;

            categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionIdAsString &&
                            x.Value == optionValue &&
                            (x.TranslationId == translationIdAsString || x.TranslationId == null))
                .OrderBy(x => x.TranslationId == null)
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

        public void RemoveOptionsForQuestionnaire(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var optionsToDelete = this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString).ToList();
            this.optionsStorage.Remove(optionsToDelete);
        }

        public bool IsEmpty() => this.optionsStorage.FirstOrDefault() == null;

        public void StoreOptionsForQuestion(QuestionnaireIdentity questionnaireIdentity, Guid questionId, List<Answer> answers, List<TranslationDto> translations)
        {
            var questionIdAsString = questionId.FormatGuid();
            var questionnaireIdAsString = questionnaireIdentity.ToString();

            var optionsToSave = new List<OptionView>();

            int index = 0;

            foreach (var answer in answers)
            {
                decimal value = answer.GetParsedValue();
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
                    SearchTitle = answer.AnswerText?.ToLower(),
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
                        QuestionId = questionIdAsString,
                        Value = value,
                        ParentValue = parentValue,
                        Title = y.Value,
                        SearchTitle = y.Value?.ToLower(),
                        SortOrder = ++index,
                        TranslationId = y.TranslationId.FormatGuid()
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