using System;
using System.Collections.Generic;
using System.Linq;
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
                .AsNoTracking()
                .Include(r => r.References)
                .Where(h => h.QuestionnaireId == sQuestionnaireId);

            if (isAdmin == false)
            {
                // NOTE: List<T> is used on purpose. For an array the C# 14+ compiler binds Contains
                // to MemoryExtensions.Contains(ReadOnlySpan<T>, T), which puts an op_Implicit call
                // into the expression tree that the ORM cannot translate.
                var adminUsers = (await userManager.GetUsersInRoleAsync(SimpleRoleEnum.Administrator))
                    .Select(u => u.Id).ToList();

                query = query.Where(h => !(h.ActionType == QuestionnaireActionType.ImportToHq && adminUsers.Contains(h.UserId)));
            }

            QuestionnaireChangeRecord[] questionnaireHistory;
            int count;
            var normalizedSearch = NormalizeSearch(search);

            if (normalizedSearch != null)
            {
                var matcher = CreateMatcher(normalizedSearch, searchWholeWord);
                var matchedEntityIds = ResolveMatchingEntityIds(questionnaire, matcher, searchIdsOnly);
                var questionnaireIdMatched = matchedEntityIds.Contains(questionnaire.PublicKey);
                var matchedNonQuestionnaireEntityIds = matchedEntityIds
                    .Where(x => x != questionnaire.PublicKey)
                    .ToList();

                if (searchWholeWord)
                {
                    query = query.Where(h =>
                        ((h.TargetItemType == QuestionnaireItemType.Questionnaire && questionnaireIdMatched && h.TargetItemId == questionnaire.PublicKey)
                         || (h.TargetItemType != QuestionnaireItemType.Questionnaire && matchedNonQuestionnaireEntityIds.Contains(h.TargetItemId))) ||
                        h.References.Any(r =>
                            (r.ReferenceType == QuestionnaireItemType.Questionnaire && questionnaireIdMatched && r.ReferenceId == questionnaire.PublicKey)
                            || (r.ReferenceType != QuestionnaireItemType.Questionnaire && matchedNonQuestionnaireEntityIds.Contains(r.ReferenceId))) ||
                        (!searchIdsOnly &&
                         ((h.TargetItemTitle != null && Regex.IsMatch(h.TargetItemTitle, $@"\m{Regex.Escape(normalizedSearch)}\M", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) ||
                          (h.TargetItemNewTitle != null && Regex.IsMatch(h.TargetItemNewTitle, $@"\m{Regex.Escape(normalizedSearch)}\M", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) ||
                          h.References.Any(r => r.ReferenceTitle != null && Regex.IsMatch(r.ReferenceTitle, $@"\m{Regex.Escape(normalizedSearch)}\M", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))));
                }
                else
                {
                    query = query.Where(h =>
                        ((h.TargetItemType == QuestionnaireItemType.Questionnaire && questionnaireIdMatched && h.TargetItemId == questionnaire.PublicKey)
                         || (h.TargetItemType != QuestionnaireItemType.Questionnaire && matchedNonQuestionnaireEntityIds.Contains(h.TargetItemId))) ||
                        h.References.Any(r =>
                            (r.ReferenceType == QuestionnaireItemType.Questionnaire && questionnaireIdMatched && r.ReferenceId == questionnaire.PublicKey)
                            || (r.ReferenceType != QuestionnaireItemType.Questionnaire && matchedNonQuestionnaireEntityIds.Contains(r.ReferenceId))) ||
                        (!searchIdsOnly &&
                         ((h.TargetItemTitle != null && h.TargetItemTitle.ToLower().Contains(normalizedSearch)) ||
                          (h.TargetItemNewTitle != null && h.TargetItemNewTitle.ToLower().Contains(normalizedSearch)) ||
                          h.References.Any(r => r.ReferenceTitle != null && r.ReferenceTitle.ToLower().Contains(normalizedSearch)))));
                }

                count = await query.CountAsync();
                questionnaireHistory = await query
                    .OrderByDescending(h => h.Sequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToArrayAsync();
            }
            else
            {
                count = await query.CountAsync();
                questionnaireHistory = await query
                    .OrderByDescending(h => h.Sequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToArrayAsync();
            }

            var userId = user.GetId();

            return new QuestionnaireChangeHistory(questionnaireId, questionnaire.Title,
                questionnaireHistory.Select(h =>
                    CreateQuestionnaireChangeHistoryWebItem(questionnaire, h, userId))
                    .ToList(), page, count, pageSize, normalizedSearch, searchIdsOnly, searchWholeWord);
        }

        private static Func<string?, bool> CreateMatcher(string search, bool wholeWord)
        {
            if (!wholeWord)
                return value => !string.IsNullOrWhiteSpace(value)
                    && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            var pattern = $@"\b{Regex.Escape(search)}\b";
            return value => !string.IsNullOrWhiteSpace(value)
                && Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string? NormalizeSearch(string? search)
        {
            return string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim().ToLowerInvariant();
        }

        private static List<Guid> ResolveMatchingEntityIds(QuestionnaireDocument questionnaire,
            Func<string?, bool> matcher, bool searchIdsOnly)
        {
            var matchedEntityIds = questionnaire.Find<IQuestionnaireEntity>()
                .Where(entity => matcher(searchIdsOnly ? entity.GetVariable() : entity.GetTitle()))
                .Select(entity => entity.PublicKey)
                .ToList();

            var questionnaireValue = searchIdsOnly ? questionnaire.VariableName : questionnaire.Title;
            if (matcher(questionnaireValue))
                matchedEntityIds.Add(questionnaire.PublicKey);

            return matchedEntityIds;
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
