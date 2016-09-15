using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public interface IPdfFactory
    {
        PdfQuestionnaireModel Load(Guid questionnaireId, Guid requestedByUserId, string requestedByUserName);
        string LoadQuestionnaireTitle(Guid questionnaireId);
    }

    public class PdfFactory : IPdfFactory
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IReadSideRepositoryReader<AccountDocument> accountsDocumentReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;
        private readonly PdfSettings pdfSettings;

        public PdfFactory(
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IQueryableReadSideRepositoryReader<QuestionnaireChangeRecord> questionnaireChangeHistoryStorage, 
            IReadSideRepositoryReader<AccountDocument> accountsDocumentReader,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage, 
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage, 
            PdfSettings pdfSettings)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireChangeHistoryStorage = questionnaireChangeHistoryStorage;
            this.accountsDocumentReader = accountsDocumentReader;
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.sharedPersonsStorage = sharedPersonsStorage;
            this.pdfSettings = pdfSettings;
        }

        public PdfQuestionnaireModel Load(Guid questionnaireId, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            if (questionnaire == null || questionnaire.IsDeleted)
            {
                return null;
            }

            questionnaire.ConnectChildrenWithParent();

            var sharedPersons = sharedPersonsStorage.GetById(questionnaireId)?.SharedPersons ?? new List<SharedPerson>();

            var modificationStatisticsByUsers = questionnaireChangeHistoryStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId.FormatGuid())
                .GroupBy(x => new { x.UserId, x.UserName })
                .Select(grouping => new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = grouping.Key.UserId,
                    Date = grouping.Max(x => x.Timestamp),
                    Name = grouping.Key.UserName,
                })).ToList();

            var allItems = questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();

            var pdfView = new PdfQuestionnaireModel(questionnaire, pdfSettings)
            {
                Requested = new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = requestedByUserId,
                    Name = requestedByUserName,
                    Date = DateTime.Now
                },
                Created = new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = questionnaire.CreatedBy ?? Guid.Empty,
                    Name = questionnaire.CreatedBy.HasValue
                        ? accountsDocumentReader.GetById(questionnaire.CreatedBy.Value)?.UserName
                        : string.Empty,
                    Date = questionnaire.CreationDate
                },
                LastModified = modificationStatisticsByUsers.OrderByDescending(x => x.Date).First(),
                SharedPersons = sharedPersons.Select(person => new PdfQuestionnaireModel.ModificationStatisticsByUser
                {
                    UserId = person.Id,
                    Name = accountsDocumentReader.GetById(person.Id)?.UserName,
                    Date = modificationStatisticsByUsers.FirstOrDefault(x => x.UserId == person.Id)?.Date
                }).Where(sharedPerson => sharedPerson.Name != requestedByUserName),
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
