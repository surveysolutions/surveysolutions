using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class QuestionnaireRepository : IPlainAggregateRootRepository<Questionnaire>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireRepository(IServiceLocator serviceLocator,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.serviceLocator = serviceLocator;
            this.questionnaireStorage = questionnaireStorage;
        }

        public Questionnaire Get(Guid aggregateId)
        {
            var questionnaireDocument = this.questionnaireStorage.GetById(aggregateId.FormatGuid());
            var questionnaire = this.serviceLocator.GetInstance<Questionnaire>();
            questionnaire.Initialize(aggregateId, questionnaireDocument);
            return questionnaire;
        }

        public void Save(Questionnaire aggregateRoot)
        {
            var questionnaireDocument = aggregateRoot.QuestionnaireDocument;
            this.questionnaireStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey.FormatGuid());
        }
    }
}