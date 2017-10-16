using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public interface IPdfFactory
    {
        PdfQuestionnaireModel Load(string questionnaireId, Guid requestedByUserId, string requestedByUserName);
        string LoadQuestionnaireTitle(Guid questionnaireId);
    }

    public class PdfFactory : IPdfFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IQuestionnaireTranslator translator;
        private readonly ITranslationsService translationService;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IPlainStorageAccessor<Aggregates.User> accountsStorage;
        private readonly PdfSettings pdfSettings;

        public PdfFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage, 
            IPlainStorageAccessor<Aggregates.User> accountsStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage,
            IQuestionnaireTranslator translator,
            ITranslationsService translationService,
            PdfSettings pdfSettings)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.accountsStorage = accountsStorage;
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.translator = translator;
            this.translationService = translationService;
            this.pdfSettings = pdfSettings;
        }

        public PdfQuestionnaireModel Load(string questionnaireId, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            
            if (questionnaire == null || questionnaire.IsDeleted)
            {
                return null;
            }
            
            if (questionnaire.DefaultTranslation != null)
            {
                var translation = translationService.Get(questionnaire.PublicKey, questionnaire.DefaultTranslation.Value);
                questionnaire = translator.Translate(questionnaire, translation);
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
                Date = DateTime.Now
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
                UserId = person.UserId,
                Name = this.accountsStorage.GetById(person.UserId.FormatGuid())?.UserName,
                Date = modificationStatisticsByUsers.FirstOrDefault(x => x.UserId == person.UserId)?.Date
            }).Where(sharedPerson => sharedPerson.Name != requestedByUserName);

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
                QuestionsWithLongOptionsList = Find<IQuestion>(allItems, x => x.Answers?.Count > this.pdfSettings.OptionsExcerptCount).ToList(),
                VariableWithLongExpressions = Find<IVariable>(allItems, x => x.Expression?.Length > this.pdfSettings.VariableExpressionExcerptLength).ToList()
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
