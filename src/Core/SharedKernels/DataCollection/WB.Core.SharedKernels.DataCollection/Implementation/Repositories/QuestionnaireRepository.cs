using System;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly IEventStore eventStore;
        private readonly IDomainRepository domainRepository;

        public QuestionnaireRepository(IEventStore eventStore, IDomainRepository domainRepository)
        {
            this.eventStore = eventStore;
            this.domainRepository = domainRepository;
        }

        public IQuestionnaire GetQuestionnaire(Guid id)
        {
            return this.GetQuestionnaireImpl(id);
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            return this.GetQuestionnaireImpl(id, version);
        }

        private IQuestionnaire GetQuestionnaireImpl(Guid id, long? version = null)
        {
            CommittedEventStream eventStream = eventStore.ReadFrom(id, long.MinValue, version ?? long.MaxValue);
            AggregateRoot aggregateRoot = domainRepository.Load(typeof (Questionnaire), null, eventStream);
            return (Questionnaire) aggregateRoot;
        }
    }
}