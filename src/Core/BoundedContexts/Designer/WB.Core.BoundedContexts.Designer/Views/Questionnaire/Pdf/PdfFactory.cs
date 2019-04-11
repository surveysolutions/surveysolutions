﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public interface IPdfFactory
    {
        PdfQuestionnaireModel Load(string questionnaireId, Guid requestedByUserId, string requestedByUserName, Guid? translation, bool useDefaultTranslation);
        string LoadQuestionnaireTitle(Guid questionnaireId);
    }

    public class PdfFactory : IPdfFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly ITranslationsService translationService;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IIdentityService accountsStorage;
        private readonly PdfSettings pdfSettings;
        private readonly IQuestionnaireTranslator questionnaireTranslator;

        public PdfFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage, 
            IIdentityService accountsStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage,
            ITranslationsService translationService,
            IOptions<PdfSettings> pdfSettings,
            IQuestionnaireTranslator questionnaireTranslator)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.accountsStorage = accountsStorage;
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.translationService = translationService;
            this.pdfSettings = pdfSettings.Value;
            this.questionnaireTranslator = questionnaireTranslator;
        }

        public PdfQuestionnaireModel Load(string questionnaireId, Guid requestedByUserId, string requestedByUserName, Guid? translation, bool useDefaultTranslation)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            
            if (questionnaire == null || questionnaire.IsDeleted)
            {
                return null;
            }

            if (translation.HasValue)
            {
                var translationMetadata = questionnaire.Translations.FirstOrDefault(t => t.Id == translation.Value);
                if (translationMetadata == null)
                    throw new ArgumentException("Questionnaire doesn't contains translation: " + translation);

                var translationData = translationService.Get(questionnaire.PublicKey, translationMetadata.Id);
                questionnaire = questionnaireTranslator.Translate(questionnaire, translationData);
            } 
            else if (useDefaultTranslation && questionnaire.DefaultTranslation != null)
            {
                var translationData = translationService.Get(questionnaire.PublicKey, questionnaire.DefaultTranslation.Value);
                questionnaire = questionnaireTranslator.Translate(questionnaire, translationData);
            }

            var listItem = this.questionnaireListViewItemStorage.GetById(questionnaireId);
            var sharedPersons =  listItem.SharedPersons;

            var modificationStatisticsByUsers = questionnaireChangeHistoryStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId)
                .GroupBy(x => new { x.UserId, x.UserName })
                .Select(grouping => new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = grouping.Key.UserId,
                    Date = grouping.Max(x => x.Timestamp),
                    Name = grouping.Key.UserName,
                })).ToList();

            var allItems = questionnaire.Children.SelectMany(x => x.TreeToEnumerable(g => g.Children)).ToList();

            var modificationStatisticsByUser = new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = requestedByUserId,
                Name = requestedByUserName,
                Date = DateTime.UtcNow
            };

            var statisticsByUser = new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = questionnaire.CreatedBy ?? Guid.Empty,
                Name = questionnaire.CreatedBy.HasValue
                    ? this.accountsStorage.GetById(questionnaire.CreatedBy.Value.FormatGuid())?.UserName
                    : string.Empty,
                Date = questionnaire.CreationDate
            };
            var lastModified = modificationStatisticsByUsers.OrderByDescending(x => x.Date).FirstOrDefault();
            var statisticsByUsers = sharedPersons.Select(person => new PdfQuestionnaireModel.ModificationStatisticsByUser
            {
                UserId = person.UserId.ParseGuid().Value,
                Name = this.accountsStorage.GetById(person.UserId)?.UserName,
                Date = modificationStatisticsByUsers.FirstOrDefault(x => x.UserId.FormatGuid() == person.UserId)?.Date
            });

            if (questionnaire.CreatedBy == requestedByUserId)
                statisticsByUsers = statisticsByUsers.Where(sharedPerson => sharedPerson.Name != requestedByUserName);

            var pdfView = new PdfQuestionnaireModel(questionnaire, pdfSettings)
            {
                Requested = modificationStatisticsByUser,
                Created = statisticsByUser,
                LastModified = lastModified,
                SharedPersons = statisticsByUsers,
                AllItems = allItems,
                ItemsWithLongConditions = CollectEntitiesWithLongConditions(allItems, pdfSettings),
                ItemsWithLongValidations = CollectItemsWithLongValidations(allItems, pdfSettings),
                QuestionsWithLongInstructions = Find<IQuestion>(allItems, x => x.Instructions?.Length > this.pdfSettings.InstructionsExcerptLength).ToList(),
                QuestionsWithLongOptionsFilterExpression = Find<IQuestion>(allItems, x => x.Properties.OptionsFilterExpression?.Length > this.pdfSettings.VariableExpressionExcerptLength || x.LinkedFilterExpression?.Length > this.pdfSettings.VariableExpressionExcerptLength).ToList(),
                QuestionsWithLongOptionsList = Find<IQuestion>(allItems, x => x.QuestionType != QuestionType.Numeric && x.Answers?.Count > this.pdfSettings.OptionsExcerptCount).ToList(),
                VariableWithLongExpressions = Find<IVariable>(allItems, x => x.Expression?.Length > this.pdfSettings.VariableExpressionExcerptLength).ToList(),
                QuestionsWithLongSpecialValuesList = Find<IQuestion>(allItems, x => x.QuestionType == QuestionType.Numeric && x.Answers?.Count > this.pdfSettings.OptionsExcerptCount).ToList()

            };

            pdfView.FillStatistics(allItems, pdfView.Statistics);
            pdfView.Statistics.SectionsCount = questionnaire.Children.Count;
            pdfView.Statistics.GroupsCount -= pdfView.Statistics.SectionsCount;
            pdfView.Statistics.QuestionsWithEnablingConditionsCount = Find<IQuestion>(allItems, x => !string.IsNullOrWhiteSpace(x.ConditionExpression)).Count();
            pdfView.Statistics.QuestionsWithValidationConditionsCount = Find<IQuestion>(allItems, x => x.ValidationConditions.Any()).Count();
            
            return pdfView;
        }

        public string LoadQuestionnaireTitle(Guid questionnaireId)
        {
            return this.questionnaireListViewItemStorage.GetById(questionnaireId.FormatGuid()).Title;
        }

        public IEnumerable<T> Find<T>(IEnumerable<IComposite> allItems, Func<T, bool> condition) where T : IComposite
            => allItems.Where(x => x is T).Cast<T>().Where(condition);

        private List<PdfQuestionnaireModel.EntityWithLongValidation> CollectItemsWithLongValidations(List<IComposite> allItems, PdfSettings settings)
        {
            var questions = this.Find<IQuestion>(allItems, x => x.ValidationConditions.Count > 0 && x.ValidationConditions.Any(condition => condition.Expression?.Length > settings.ExpressionExcerptLength))
                .Select(x => new PdfQuestionnaireModel.EntityWithLongValidation
                {
                    Id = x.PublicKey,
                    Title = x.QuestionText,
                    VariableName = x.StataExportCaption,
                    ValidationConditions = x.ValidationConditions.ToList()
                });
            var staticTexts = this.Find<IStaticText>(allItems, x => x.ValidationConditions?.Count > 0 && x.ValidationConditions.Any(condition => condition.Expression?.Length > settings.ExpressionExcerptLength))
                .Select(x => new PdfQuestionnaireModel.EntityWithLongValidation
                {
                    Id = x.PublicKey,
                    Title = x.Text,
                    ValidationConditions = x.ValidationConditions.ToList()
                });
            var entitiesWithLongValidations = questions.Concat(staticTexts).ToList();

            int index = 1;
            entitiesWithLongValidations.ForEach(x => x.Index = index++);

            return entitiesWithLongValidations;
        }

        private List<PdfQuestionnaireModel.EntityWithLongCondition> CollectEntitiesWithLongConditions(List<IComposite> allItems, PdfSettings settings)
        {
            var questions = this.Find<IQuestion>(allItems, x => x.ConditionExpression?.Length > settings.ExpressionExcerptLength)
                .Select(x => new PdfQuestionnaireModel.EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.QuestionText,
                    VariableName = x.StataExportCaption,
                    EnablementCondition = x.ConditionExpression.Trim()
                });
            var groupsAndRosters = this.Find<IGroup>(allItems, x => x.ConditionExpression?.Length > settings.ExpressionExcerptLength)
                .Select(x => new PdfQuestionnaireModel.EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.Title,
                    EnablementCondition = x.ConditionExpression.Trim()
                });
            var staticTexts = this.Find<IStaticText>(allItems, x => x.ConditionExpression?.Length > settings.ExpressionExcerptLength)
                .Select(x => new PdfQuestionnaireModel.EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.Text,
                    EnablementCondition = x.ConditionExpression.Trim()
                });

            var entitiesWithLongConditions = questions.Concat(groupsAndRosters).Concat(staticTexts).ToList();

            int index = 1;
            entitiesWithLongConditions.ForEach(x => x.Index = index++);

            return entitiesWithLongConditions;
        }
    }
}
