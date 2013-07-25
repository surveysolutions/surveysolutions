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

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            CommittedEventStream eventStream = eventStore.ReadFrom(id, long.MinValue, version);
            AggregateRoot aggregateRoot = domainRepository.Load(typeof(Questionnaire), null, eventStream);
            return (Questionnaire) aggregateRoot;
        }
    }
}