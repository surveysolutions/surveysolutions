using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
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

        public IReadOnlyList<CategoricalQuestionOption> GetQuestionOptions(QuestionnaireIdentity questionnaireId, Guid questionId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var questionIdAsString = questionId.FormatGuid();

            var categoricalQuestionOptions = this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString && x.QuestionId == questionIdAsString)
                .Select(x => new CategoricalQuestionOption
                {
                    ParentValue = x.ParentValue,
                    Value = x.Value,
                    Title = x.Title
                })
                .OrderBy(x => x.Title)
                .ToList()
                .ToReadOnlyCollection();

            return categoricalQuestionOptions;
        }

        public async Task RemoveOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireId)
        {
            var questionnaireIdAsString = questionnaireId.ToString();
            var optionsToDelete = this.optionsStorage.Where(x => x.QuestionnaireId == questionnaireIdAsString).ToList();
            await this.optionsStorage.RemoveAsync(optionsToDelete);
        }

        public async Task StoreQuestionOptionsForQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument serializedQuestionnaireDocument)
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

        private async Task StoreOptionsForQuestionAsync(string questionnaireIdAsString, string questionIdAsString, List<Answer> answers)
        {
            var optionsToSave = new List<OptionView>();

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
                    Title = answer.AnswerText
                };

                optionsToSave.Add(optionView);
            }

            await this.optionsStorage.StoreAsync(optionsToSave);
        }
    }
}