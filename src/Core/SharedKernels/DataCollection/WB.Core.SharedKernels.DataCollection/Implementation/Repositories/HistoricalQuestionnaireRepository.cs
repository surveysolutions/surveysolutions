using System;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class HistoricalQuestionnaireRepository : IHistoricalQuestionnaireRepository
    {
        private readonly IEventStore eventStore;
        private readonly IDomainRepository domainRepository;

        public HistoricalQuestionnaireRepository(IEventStore eventStore, IDomainRepository domainRepository)
        {
            this.eventStore = eventStore;
            this.domainRepository = domainRepository;
        }

        public IQuestionnaire GetQuestionnaire(Guid id, long version)
        {
            CommittedEventStream eventStream = eventStore.ReadFrom(id, long.MinValue, version);
            AggregateRoot aggregateRoot = domainRepository.Load(typeof(Questionnaire), null, eventStream);
            return (Questionnaire) aggregateRoot;
        }
    }
}