using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireViewFactory
    {
        QuestionnaireView? Load(QuestionnaireViewInputModel input);
        QuestionnaireView? Load(QuestionnaireRevision revision);

        bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId);

        bool HasUserAccessToRevertQuestionnaire(Guid questionnaireId, Guid userId);
        bool HasUserAccessToEditComments(QuestionnaireChangeRecord changeRecord, QuestionnaireDocument questionnaire, Guid userId);
        bool HasUserAccessToEditComments(Guid revisionId, Guid userId);
    }

    public class QuestionnaireViewFactory : IQuestionnaireViewFactory
    {
        private readonly IDesignerQuestionnaireStorage questionnaireStorage;
        private readonly DesignerDbContext dbContext;

        public QuestionnaireViewFactory(
            IDesignerQuestionnaireStorage questionnaireStorage,
            DesignerDbContext dbContext)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.dbContext = dbContext;
        }

        public QuestionnaireView? Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);
            var sharedPersons = this.GetSharedPersons(input.QuestionnaireId);
            return doc == null ? null : new QuestionnaireView(doc, sharedPersons);
        }

        public QuestionnaireView? Load(QuestionnaireRevision revision)
        {
            var doc = this.questionnaireStorage.Get(revision);
            var sharedPersons = this.GetSharedPersons(revision.QuestionnaireId);
            return doc == null ? null : new QuestionnaireView(doc, sharedPersons);
        }

        public bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.Get(questionnaireId);
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var questionnaireListItem = this.dbContext.Questionnaires
                .Where(x => x.QuestionnaireId == questionnaireId.FormatGuid())
                .Include(x => x.SharedPersons).FirstOrDefault();

            if (questionnaireListItem == null)
                return false;

            if (questionnaireListItem.IsPublic)
                return true;

            if (questionnaireListItem.SharedPersons.Any(x => x.UserId == userId))
                return true;

            return false;
        }

        public bool HasUserAccessToRevertQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.Get(questionnaireId);
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var listViewItem = this.dbContext.Questionnaires.Include(x => x.SharedPersons)
                .FirstOrDefault(x => x.QuestionnaireId == questionnaireId.FormatGuid());

            if (listViewItem == null) return false;
            
            var sharedPersons = listViewItem.SharedPersons;
            return sharedPersons.Any(x => x.UserId == userId && x.ShareType == ShareType.Edit);
        }

        private List<SharedPersonView> GetSharedPersons(Guid questionnaireId)
        {
            var listViewItem = this.dbContext.SharedPersons.Where(x => x.QuestionnaireId 
                == questionnaireId.FormatGuid()).ToList();

            var sharedPersons = listViewItem
                .Select(x => new SharedPersonView
                {
                    Email = x.Email,
                    IsOwner = x.IsOwner,
                    Login = this.dbContext.Users.Find(x.UserId)?.UserName ?? string.Empty,
                    ShareType = x.ShareType,
                    UserId = x.UserId
                });
            return sharedPersons.ToList();
        }

        private QuestionnaireDocument? GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            try
            {
                var doc = this.questionnaireStorage.Get(input.QuestionnaireId);
                if (doc == null || doc.IsDeleted)
                {
                    return null;
                }

                return doc;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public bool HasUserAccessToEditComments(Guid revisionId, Guid userId)
        {
            var changeRecord = this.dbContext.QuestionnaireChangeRecords.Single(
                q => q.QuestionnaireChangeRecordId == revisionId.FormatGuid());

            var questionnaire = this.questionnaireStorage.Get(Guid.Parse(changeRecord.QuestionnaireId));
            
            return questionnaire != null && HasUserAccessToEditComments(changeRecord, questionnaire, userId);
        }

        public bool HasUserAccessToEditComments(
            QuestionnaireChangeRecord changeRecord,
            QuestionnaireDocument questionnaire, 
            Guid userId)
        {
            if(changeRecord.ActionType == QuestionnaireActionType.ImportToHq)
            {
                return questionnaire.CreatedBy == userId;
            }

            return true;
        }
    }
}
