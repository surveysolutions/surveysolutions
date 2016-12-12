using System;
using System.Linq;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class QuestionnaireRepository : IPlainAggregateRootRepository<Questionnaire>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListItems;
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireRepository(IServiceLocator serviceLocator,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListItems)
        {
            this.serviceLocator = serviceLocator;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireListItems = questionnaireListItems;
        }

        public Questionnaire Get(Guid aggregateId)
        {
            var questionnaireDocument = this.questionnaireStorage.GetById(aggregateId.FormatGuid());

            if (questionnaireDocument == null)
                return null;

            var questionnaireListItem = this.questionnaireListItems.GetById(aggregateId.FormatGuid());
            var personsCollection = questionnaireListItem?.SharedPersons ?? Enumerable.Empty<SharedPerson>();
            var questionnaire = this.serviceLocator.GetInstance<Questionnaire>();
            questionnaire.Initialize(aggregateId, questionnaireDocument, personsCollection);
            return questionnaire;
        }

        public void Save(Questionnaire aggregateRoot)
        {
            var questionnaireDocument = aggregateRoot.QuestionnaireDocument;
            this.questionnaireStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey.FormatGuid());
        }
    }
}