using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireViewFactory
    {
        QuestionnaireView Load(QuestionnaireViewInputModel input);

        bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId);
    }

    public class QuestionnaireViewFactory : IQuestionnaireViewFactory
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;

        public QuestionnaireViewFactory(
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersonsStorage = sharedPersonsStorage;
        }

        public QuestionnaireView Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);
            return doc == null ? null : new QuestionnaireView(doc);
        }

        public bool HasUserAccessToQuestionnaire(Guid questionnaireId, Guid userId)
        {
            var questionnaire = this.questionnaireStorage.GetById(questionnaireId);
            if (questionnaire == null || questionnaire.IsDeleted)
                return false;

            if (questionnaire.CreatedBy == userId)
                return true;

            var sharedPersons = sharedPersonsStorage.GetById(questionnaireId.FormatGuid())?.SharedPersons ?? new List<SharedPerson>();
            return sharedPersons.Any(x => x.Id == userId);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            try
            {
                var doc = this.questionnaireStorage.GetById(input.QuestionnaireId);
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