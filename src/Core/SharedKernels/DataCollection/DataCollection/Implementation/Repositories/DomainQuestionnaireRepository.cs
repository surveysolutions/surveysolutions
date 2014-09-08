using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class DomainQuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        private readonly IDomainRepository domainRepository;
        private readonly Dictionary<string, IQuestionnaire> historicalQuestionnaireCache = new Dictionary<string, IQuestionnaire>();

        public DomainQuestionnaireRepository(IDomainRepository domainRepository, IEventStore eventStore,
                                       ISnapshotStore snapshotStore)
        {
            this.eventStore = eventStore;
            this.domainRepository = domainRepository;
            this.snapshotStore = snapshotStore;
        }

        public IQuestionnaire GetQuestionnaire(Guid id)
        {
            return this.GetHistoricalQuestionnaireImpl(id);
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            string cacheKey = string.Format("{0}---{1}", id, version);

            if (!this.historicalQuestionnaireCache.ContainsKey(cacheKey))
            {
                IQuestionnaire historicalQuestionnaire = this.GetHistoricalQuestionnaireImpl(id, version);

                if (historicalQuestionnaire == null)
                    return null; // does not need to store not existing result because questionnaire with asked version and id might probably appear in future

                this.historicalQuestionnaireCache[cacheKey] = historicalQuestionnaire;
            }

            return this.historicalQuestionnaireCache[cacheKey];
        }

        private IQuestionnaire GetHistoricalQuestionnaireImpl(Guid id, long? version = null)
        {
            long maxEvent = long.MaxValue;
            Snapshot snapshot = null;
            long minVersion = long.MinValue;
            snapshot = snapshotStore.GetSnapshot(id, maxEvent);
            if (snapshot != null)
            {
                minVersion = snapshot.Version + 1;
            }
            var eventStream = eventStore.ReadFrom(id, minVersion, maxEvent);
            var aggregateRoot = (Questionnaire) domainRepository.Load(typeof (Questionnaire), snapshot, eventStream);

            if (!eventStream.IsEmpty)
                snapshotStore.SaveShapshot(new Snapshot(id, eventStream.CurrentSourceVersion, aggregateRoot.CreateSnapshot()));

            if(version.HasValue)
                return aggregateRoot.GetHistoricalQuestionnaire(version.Value);
            return aggregateRoot.GetQuestionnaire();
        }
    }
}