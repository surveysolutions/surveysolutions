using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    internal class QuestionnaireChangeHistoryFactory : IQuestionnaireChangeHistoryFactory
    {
        private readonly DesignerDbContext dbContext;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage;
        private readonly IUserManager userManager;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public QuestionnaireChangeHistoryFactory(
            DesignerDbContext dbContext,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentStorage,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IUserManager userManager)
        {
            this.dbContext = dbContext;
            this.questionnaireDocumentStorage = questionnaireDocumentStorage;
            this.userManager = userManager;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        public async Task<QuestionnaireChangeHistory?> LoadAsync(Guid questionnaireId, int page, int pageSize, IPrincipal user,
            string? search = null, bool searchIdsOnly = false, bool searchWholeWord = false)
        {
            var questionnaire = questionnaireDocumentStorage.GetById(questionnaireId.FormatGuid());

            if (questionnaire == null)
                return null;

            var sQuestionnaireId = questionnaireId.FormatGuid();

            var isAdmin = user.IsAdmin();

            IQueryable<QuestionnaireChangeRecord> query = this.dbContext.QuestionnaireChangeRecords
                .Include(r => r.References)
                .Where(h => h.QuestionnaireId == sQuestionnaireId);

            if (isAdmin == false)
            {
                var adminUsers = (await userManager.GetUsersInRoleAsync(SimpleRoleEnum.Administrator))
                    .Select(u => u.Id).ToArray();

                query = query.Where(h => !(h.ActionType == QuestionnaireActionType.ImportToHq && adminUsers.Contains(h.UserId)));
            }

            var questionnaireHistory = await query
                .OrderByDescending(h => h.Sequence)
                .ToArrayAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var matcher = CreateMatcher(search.Trim(), searchWholeWord);
                questionnaireHistory = questionnaireHistory
                    .Where(h => MatchesRecord(questionnaire, h, matcher, searchIdsOnly))
                    .ToArray();
            }

            var count = questionnaireHistory.Length;
            questionnaireHistory = questionnaireHistory
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();
            var userId = user.GetId();

            return new QuestionnaireChangeHistory(questionnaireId, questionnaire.Title,
                questionnaireHistory.Select(h => 
                    CreateQuestionnaireChangeHistoryWebItem(questionnaire, h, userId))
                    .ToList(), page, count, pageSize, search, searchIdsOnly, searchWholeWord);
        }

        private static Func<string?, bool> CreateMatcher(string search, bool wholeWord)
        {
            if (!wholeWord)
                return value => !string.IsNullOrWhiteSpace(value)
                    && value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;

            var pattern = $@"\b{Regex.Escape(search)}\b";
            return value => !string.IsNullOrWhiteSpace(value)
                && Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool MatchesRecord(QuestionnaireDocument questionnaire, QuestionnaireChangeRecord record,
            Func<string?, bool> matcher, bool searchIdsOnly)
        {
            var targetMatch = searchIdsOnly
                ? MatchEntityId(questionnaire, record.TargetItemId, record.TargetItemType, record.TargetItemTitle, matcher)
                : MatchEntityText(questionnaire, record.TargetItemId, record.TargetItemType, record.TargetItemTitle, matcher)
                    || matcher(record.TargetItemNewTitle);

            if (targetMatch)
                return true;

            return record.References.Any(reference => searchIdsOnly
                ? MatchEntityId(questionnaire, reference.ReferenceId, reference.ReferenceType, reference.ReferenceTitle, matcher)
                : MatchEntityText(questionnaire, reference.ReferenceId, reference.ReferenceType, reference.ReferenceTitle, matcher));
        }

        private static bool MatchEntityId(QuestionnaireDocument questionnaire, Guid entityId, QuestionnaireItemType entityType,
            string? fallbackValue, Func<string?, bool> matcher)
        {
            var candidate = ResolveEntityId(questionnaire, entityId, entityType) ?? fallbackValue;
            return matcher(candidate);
        }

        private static bool MatchEntityText(QuestionnaireDocument questionnaire, Guid entityId, QuestionnaireItemType entityType,
            string? fallbackValue, Func<string?, bool> matcher)
        {
            var resolvedText = ResolveEntityText(questionnaire, entityId, entityType);
            if (!string.IsNullOrWhiteSpace(resolvedText))
                return matcher(resolvedText);

            if (entityType == QuestionnaireItemType.Variable)
                return false;

            return matcher(fallbackValue);
        }

        private static string? ResolveEntityId(QuestionnaireDocument questionnaire, Guid entityId, QuestionnaireItemType entityType)
        {
            if (entityType == QuestionnaireItemType.Questionnaire)
                return questionnaire.VariableName;

            var entity = questionnaire.FirstOrDefault<IComposite>(item => item.PublicKey == entityId) as IQuestionnaireEntity;
            return entity?.GetVariable();
        }

        private static string? ResolveEntityText(QuestionnaireDocument questionnaire, Guid entityId, QuestionnaireItemType entityType)
        {
            if (entityType == QuestionnaireItemType.Questionnaire)
                return questionnaire.Title;

            var entity = questionnaire.FirstOrDefault<IComposite>(item => item.PublicKey == entityId) as IQuestionnaireEntity;
            return entity?.GetTitle();
        }

        private QuestionnaireChangeHistoricalRecord CreateQuestionnaireChangeHistoryWebItem(
            QuestionnaireDocument questionnaire,
            QuestionnaireChangeRecord revision,
            Guid userId)
        {
            var references =
                revision.References.Select(
                    r => CreateQuestionnaireChangeHistoryReference(questionnaire, r)).ToList();
            
            var canEditComment = questionnaireViewFactory.HasUserAccessToEditComments(revision, questionnaire, userId);

            return new QuestionnaireChangeHistoricalRecord(
                revision.QuestionnaireChangeRecordId,
                revision.UserName,
                revision.Timestamp,
                revision.ActionType,
                revision.TargetItemId,
                GetItemParentId(questionnaire, revision.TargetItemId),
                revision.TargetItemTitle,
                revision.TargetItemType,
                revision.TargetItemNewTitle,
                revision.AffectedEntriesCount,
                revision.Patch != null || revision.ResultingQuestionnaireDocument != null,
                revision.TargetItemDateTime,
                references,
                revision.Meta?.Comment,
                revision.Meta?.Hq?.Version,
                revision.Meta?.Hq?.QuestionnaireVersion,
                canEditComment)
            {
                HqUserName = revision.Meta?.Hq?.ImporterLogin,
                Sequence = revision.Sequence
            };
        }

        private QuestionnaireChangeHistoricalRecordReference CreateQuestionnaireChangeHistoryReference(
            QuestionnaireDocument questionnaire,
            QuestionnaireChangeReference questionnaireChangeReference)
        {
            return new QuestionnaireChangeHistoricalRecordReference(
                questionnaireChangeReference.ReferenceId,
                GetItemParentId(questionnaire, questionnaireChangeReference.ReferenceId),
                questionnaireChangeReference.ReferenceTitle,
                questionnaireChangeReference.ReferenceType,
                IsQuestionnaireChangeHistoryReferenceExists(questionnaire, questionnaireChangeReference.ReferenceId,
                    questionnaireChangeReference.ReferenceType));
        }

        private Guid? GetItemParentId(QuestionnaireDocument questionnaire, Guid itemId)
        {
            IComposite? item = questionnaire.FirstOrDefault<IComposite>(g => g.PublicKey == itemId);
            if (item == null)
                return null;

            while (item != null && item.GetParent()?.GetType() != typeof(QuestionnaireDocument))
            {
                item = item.GetParent();
            }
            return item?.PublicKey;
        }
        
        private bool IsQuestionnaireChangeHistoryReferenceExists(QuestionnaireDocument questionnaire, Guid itemId,
            QuestionnaireItemType type)
        {
            switch (type)
            {
                case QuestionnaireItemType.Section:
                case QuestionnaireItemType.Question:
                case QuestionnaireItemType.Roster:
                case QuestionnaireItemType.StaticText:
                case QuestionnaireItemType.Variable:
                    return questionnaire.FirstOrDefault<IComposite>(g => g.PublicKey == itemId) != null;
                case QuestionnaireItemType.Person:
                    return true;
                case QuestionnaireItemType.Questionnaire:
                    var questionnaireItem = questionnaireDocumentStorage.GetById(itemId.FormatGuid());
                    return questionnaireItem != null && !questionnaireItem.IsDeleted;
                case QuestionnaireItemType.Macro:
                case QuestionnaireItemType.LookupTable:
                    return false;
            }
            return false;
        }
    }
}
