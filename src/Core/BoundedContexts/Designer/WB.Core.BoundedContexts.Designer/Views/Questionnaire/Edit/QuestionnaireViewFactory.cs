using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireViewFactory
    {
        QuestionnaireView Load(QuestionnaireViewInputModel input);

        bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId);

        bool HasUserAccessToRevertQuestionnaire(Guid questionnaireId, Guid userId);
    }

    public class QuestionnaireViewFactory : IQuestionnaireViewFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage;
        private readonly IAccountRepository accountRepository;

        public QuestionnaireViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage, 
            IAccountRepository accountRepository)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.listItemStorage = listItemStorage;
            this.accountRepository = accountRepository;
        }

        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);
            var sharedPersons = this.GetSharedPersons(input.QuestionnaireId);
            return doc == null ? null : new QuestionnaireView(doc, sharedPersons);
        }

        public bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var questionnaireListItem = this.listItemStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaireListItem.IsPublic)
                return true;

            if (questionnaireListItem.SharedPersons.Any(x => x.UserId == userId))
                return true;

            return false;
        }

        public bool HasUserAccessToRevertQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var listViewItem = this.listItemStorage.GetById(questionnaireId.FormatGuid());

            var sharedPersons = listViewItem.SharedPersons;
            if (sharedPersons.Any(x => x.UserId == userId && x.ShareType == ShareType.Edit))
                return true;

            return false;
        }

        private List<SharedPersonView> GetSharedPersons(Guid questionnaireId)
        {
            var listViewItem = this.listItemStorage.GetById(questionnaireId.FormatGuid());
            var sharedPersons = listViewItem.SharedPersons
                .Select(x => new SharedPersonView
                {
                    Email = x.Email,
                    IsOwner = x.IsOwner,
                    Login = accountRepository.GetByProviderKey(x.UserId)?.UserName ?? string.Empty,
                    ShareType = x.ShareType,
                    UserId = x.UserId
                });
            return sharedPersons.ToList();
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            try
            {
                var doc = this.questionnaireStorage.GetById(input.QuestionnaireId.FormatGuid());
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
    }
}
