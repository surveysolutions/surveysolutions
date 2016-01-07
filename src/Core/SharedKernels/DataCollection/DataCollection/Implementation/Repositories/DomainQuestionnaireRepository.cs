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

        public DomainQuestionnaireRepository(IDomainRepository domainRepository, IEventStore eventStore,
                                       ISnapshotStore snapshotStore)
        {
            this.eventStore = eventStore;
            this.domainRepository = domainRepository;
            this.snapshotStore = snapshotStore;
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            return this.GetHistoricalQuestionnaireImpl(id, (int)version);
        }

        private IQuestionnaire GetHistoricalQuestionnaireImpl(Guid id, int version)
        {
            int maxEvent = int.MaxValue;
            Snapshot snapshot = null;
            int minVersion = int.MinValue;
            snapshot = snapshotStore.GetSnapshot(id, maxEvent);
            if (snapshot != null)
            {
                minVersion = snapshot.Version + 1;

                if (snapshot.Version >= version)
                {
                    var questionnaire = (Questionnaire)this.domainRepository.Load(typeof(Questionnaire), snapshot, new CommittedEventStream(snapshot.EventSourceId));
                    return questionnaire.GetHistoricalQuestionnaire(version);
                }
            }

            var eventStream = eventStore.ReadFrom(id, minVersion, maxEvent);
            var aggregateRoot = (Questionnaire) domainRepository.Load(typeof (Questionnaire), snapshot, eventStream);

            if (!eventStream.IsEmpty)
                snapshotStore.SaveShapshot(new Snapshot(id, eventStream.CurrentSourceVersion, aggregateRoot.CreateSnapshot()));

            return aggregateRoot.GetHistoricalQuestionnaire(version);
        }
    }
}