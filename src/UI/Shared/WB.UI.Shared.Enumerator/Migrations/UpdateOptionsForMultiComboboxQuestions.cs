using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.Shared.Enumerator.Migrations
{
    [Migration(201904081213)]
    public class UpdateOptionsForMultiComboboxQuestions : IMigration
    {
        private readonly IInterviewerQuestionnaireAccessor questionnaireRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IOptionsRepository optionsRepository;
        private readonly IPlainStorage<OptionView, int?> optionsStorage;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;

        public UpdateOptionsForMultiComboboxQuestions(
            IInterviewerQuestionnaireAccessor questionnaireRepository,
            IQuestionnaireStorage questionnaireStorage,
            IOptionsRepository optionsRepository,
            IPlainStorage<OptionView, int?> optionsStorage,
            IPlainStorage<TranslationInstance> translationsStorage)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.optionsRepository = optionsRepository;
            this.optionsStorage = optionsStorage;
            this.translationsStorage = translationsStorage;
        }

        public void Up()
        {
            foreach (var questionnaireIdentity in questionnaireRepository.GetAllQuestionnaireIdentities())
            {
                var questionnaire = questionnaireRepository.GetQuestionnaire(questionnaireIdentity);
                var categoricalMultiComboboxQuestions = questionnaire.Find<MultyOptionsQuestion>(x => x.IsFilteredCombobox == true && x.Answers.Any());

                if (!categoricalMultiComboboxQuestions.Any()) continue;

                foreach (var question in categoricalMultiComboboxQuestions)
                {
                    var questionnaireId = questionnaireIdentity.ToString();
                    var questionId = question.PublicKey;
                    var sQuestionId = questionId.ToString("N");

                    if (optionsStorage.Count(x => x.QuestionId == sQuestionId && x.QuestionnaireId == questionnaireId) > 0) continue;

                    var questionTranslations = translationsStorage.Where(x =>
                        x.QuestionnaireId == questionnaireId && x.QuestionnaireEntityId == questionId);

                    optionsRepository.StoreOptionsForQuestion(questionnaireIdentity, question.PublicKey,
                        question.Answers, questionTranslations.Select(ToTranslation).ToList());

                    translationsStorage.Remove(questionTranslations);

                    question.Answers = new List<Answer>();
                }

                questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version, questionnaire);
            }
        }

        private TranslationDto ToTranslation(TranslationInstance translation) => new TranslationDto
        {
            QuestionnaireEntityId = translation.QuestionnaireEntityId,
            TranslationId = translation.TranslationId,
            TranslationIndex = translation.TranslationIndex,
            Type = translation.Type,
            Value = translation.Value
        };
    }
}
