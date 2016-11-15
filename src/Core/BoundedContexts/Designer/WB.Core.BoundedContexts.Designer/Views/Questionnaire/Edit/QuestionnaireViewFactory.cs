using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
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
        private readonly IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage;

        public QuestionnaireViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> listItemStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersonsStorage = sharedPersonsStorage;
            this.listItemStorage = listItemStorage;
        }

        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);
            return doc == null ? null : new QuestionnaireView(doc);
        }

        public bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var sharedPersons = sharedPersonsStorage.GetById(questionnaireId.FormatGuid())?.SharedPersons ?? new List<SharedPerson>();
            if (sharedPersons.Any(x => x.Id == userId))
                return true;

            var questionnaireListItem = this.listItemStorage.GetById(questionnaireId.FormatGuid());
            if (questionnaireListItem.IsPublic)
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

            var sharedPersons = sharedPersonsStorage.GetById(questionnaireId.FormatGuid())?.SharedPersons ?? new List<SharedPerson>();
            if (sharedPersons.Any(x => x.Id == userId))
                return true;

            return false;
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