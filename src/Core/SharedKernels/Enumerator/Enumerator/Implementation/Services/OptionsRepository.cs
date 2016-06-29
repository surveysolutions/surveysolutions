using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

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
                .ToList().ToReadOnlyCollection();

            return categoricalQuestionOptions;
        }

        public IEnumerable<CategoricalOption> GetFilteredQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId, int? parentValue, string filter)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();
            filter = filter ?? String.Empty;
            int pagesize = 50;
            int lastLoadedSortIndex = -1;

            var parentValueAsDecimal = parentValue.HasValue ? Convert.ToDecimal(parentValue) : (decimal?) null;

            List<CategoricalOption> loadedBatch;

            do
            {
                var optionViews = this.optionsStorage
                    .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                                x.QuestionId == questionIdAsString &&
                                x.ParentValue == parentValueAsDecimal &&
                                x.Title.Contains(filter) &&
                                x.SortOrder > lastLoadedSortIndex)
                    .OrderBy(x => x.SortOrder)
                    .Take(pagesize)
                    .ToList();

                loadedBatch = optionViews.Select(x => new CategoricalOption
                {
                    ParentValue = x.ParentValue.HasValue ? Convert.ToInt32(x.ParentValue) : (int?) null,
                    Value = Convert.ToInt32(x.Value),
                    Title = x.Title
                }).ToList();

                foreach (var option in loadedBatch)
                {
                    yield return option;
                }
                lastLoadedSortIndex = optionViews.LastOrDefault()?.SortOrder ?? 0;

            } while (loadedBatch.Count > 0);

        }

        public CategoricalOption GetQuestionOption(QuestionnaireIdentity questionnaireId, Guid questionId, string optionValue)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();

            var categoricalQuestionOption = this.optionsStorage
                .Where(x => x.QuestionnaireId == questionnaireIdAsString &&
                            x.QuestionId == questionIdAsString &&
                            x.Title == optionValue)
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

        public async Task StoreQuestionOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, 
            QuestionnaireDocument serializedQuestionnaireDocument)
        {
            var questionnaireIdAsString = questionnaireIdentity.ToString();

            var questionsWithLongOptionsList = serializedQuestionnaireDocument.Find<SingleQuestion>(
                x => x.CascadeFromQuestionId.HasValue
                || (x.IsFilteredCombobox ?? false));

            foreach (var x in questionsWithLongOptionsList)
            {
                var questionIdAsString = x.PublicKey.FormatGuid();
                await this.StoreOptionsForQuestionAsync(questionnaireIdAsString, questionIdAsString, x.Answers);
            }
        }

        public bool IsEmpty() => this.optionsStorage.FirstOrDefault() == null;

        private async Task StoreOptionsForQuestionAsync(string questionnaireIdAsString, string questionIdAsString, List<Answer> answers)
        {
            var optionsToSave = new List<OptionView>();

            for (int i = 0; i < answers.Count; i++)
            {
                var answer = answers[i];

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
                    SortOrder = i
                };

                optionsToSave.Add(optionView);
            }

            await this.optionsStorage.StoreAsync(optionsToSave);
        }
    }
}