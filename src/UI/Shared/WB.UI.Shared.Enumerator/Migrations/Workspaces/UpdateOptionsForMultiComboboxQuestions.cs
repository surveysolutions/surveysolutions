using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Core;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(201904081213)]
    public class UpdateOptionsForMultiComboboxQuestions : IMigration
    {
        private readonly IInterviewerQuestionnaireAccessor questionnaireRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorage<Old.OptionView> optionsStorage;
        private readonly IPlainStorage<TranslationInstance> translationsStorage;

        public UpdateOptionsForMultiComboboxQuestions(
            ILifetimeScope lifetimeScope
            )
        {
            var migrationScope = lifetimeScope.BeginLifetimeScope(cb =>
                {
                    var assembliesDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies");
                    cb.RegisterType<InterviewerQuestionnaireAssemblyAccessor>().As<IQuestionnaireAssemblyAccessor>()
                        .WithParameter("pathToAssembliesDirectory", assembliesDirectory);
                }
            );
            this.questionnaireRepository = migrationScope.Resolve<IInterviewerQuestionnaireAccessor>();
            this.questionnaireStorage = migrationScope.Resolve<IQuestionnaireStorage>();
            this.optionsStorage = migrationScope.Resolve<IPlainStorage<Old.OptionView>>();
            this.translationsStorage = migrationScope.Resolve<IPlainStorage<TranslationInstance>>();
        }
        
        public class Old
        {
            public class OptionView : IPlainStorageEntity
            {
                [PrimaryKey]
                public string Id { get; set; }

                [Indexed]
                public string QuestionnaireId { get; set; }

                [Indexed]
                public string QuestionId { get; set; }

                public decimal Value { get; set; }

                [Indexed]
                public string Title { get; set; }

                [Indexed]
                public string SearchTitle { get; set; }

                public decimal? ParentValue { get; set; }

                public int SortOrder { get; set; }

                public string TranslationId { get; set; }
            }
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

                    StoreOptionsForQuestionOldVersion(questionnaireIdentity, question.PublicKey,
                        question.Answers, questionTranslations.Select(ToTranslation).ToList());

                    translationsStorage.Remove(questionTranslations);

                    question.Answers = new List<Answer>();
                }

                questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version, questionnaire);
            }
        }
        
        private void StoreOptionsForQuestionOldVersion(QuestionnaireIdentity questionnaireIdentity, Guid questionId, 
            List<Answer> answers, List<TranslationDto> translations)
        {
            var questionIdAsString = questionId.FormatGuid();
            var questionnaireIdAsString = questionnaireIdentity.ToString();

            var optionsToSave = new List<Old.OptionView>();

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

                var optionView = new Old.OptionView
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
                    .Select(y => new Old.OptionView
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
