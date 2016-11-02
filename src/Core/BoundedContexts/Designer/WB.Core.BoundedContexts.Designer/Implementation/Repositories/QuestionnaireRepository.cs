using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class QuestionnaireRepository : IPlainAggregateRootRepository<Questionnaire>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage;
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireRepository(IServiceLocator serviceLocator,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IPlainKeyValueStorage<QuestionnaireSharedPersons> sharedPersonsStorage)
        {
            this.serviceLocator = serviceLocator;
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersonsStorage = sharedPersonsStorage;
        }

        public Questionnaire Get(Guid aggregateId)
        {
            var questionnaireDocument = this.questionnaireStorage.GetById(aggregateId.FormatGuid());

            if (questionnaireDocument == null)
                return null;

            var sharedPersons = this.sharedPersonsStorage.GetById(aggregateId.FormatGuid());
            var personsCollection = sharedPersons?.SharedPersons ?? Enumerable.Empty<SharedPerson>();
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