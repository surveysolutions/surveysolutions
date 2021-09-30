using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
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

        public async Task<QuestionnaireChangeHistory?> LoadAsync(Guid questionnaireId, int page, int pageSize, IPrincipal user)
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

            var count = await query.CountAsync();

            var questionnaireHistory = await query
                    .OrderByDescending(h => h.Sequence)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToArrayAsync();
            var userId = user.GetId();

            return new QuestionnaireChangeHistory(questionnaireId, questionnaire.Title,
                questionnaireHistory.Select(h => 
                    CreateQuestionnaireChangeHistoryWebItem(questionnaire, h, userId))
                    .ToList(), page, count, pageSize);
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
