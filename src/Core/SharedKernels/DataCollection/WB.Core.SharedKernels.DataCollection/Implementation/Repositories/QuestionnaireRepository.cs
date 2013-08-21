using System;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly IEventStore eventStore;
        private readonly ISnapshotStore snapshotStore;
        private readonly IDomainRepository domainRepository;

        public QuestionnaireRepository(IDomainRepository domainRepository, IEventStore eventStore,
                                       ISnapshotStore snapshotStore)
        {
            this.eventStore = eventStore;
            this.domainRepository = domainRepository;
            this.snapshotStore = snapshotStore;
        }

        public IQuestionnaire GetQuestionnaire(Guid id)
        {
            return this.GetQuestionnaireImpl(id);
        }

        public IQuestionnaire GetHistoricalQuestionnaire(Guid id, long version)
        {
            IQuestionnaire historicalQuestionnaire = this.GetQuestionnaireImpl(id, version);

            if (historicalQuestionnaire.Version != version)
                return null;

            return historicalQuestionnaire;
        }

        private IQuestionnaire GetQuestionnaireImpl(Guid id, long? version = null)
        {
            long maxVersion = version.HasValue ? version.Value : long.MaxValue;
            Snapshot snapshot = null;
            long minVersion = long.MinValue;
            snapshot = snapshotStore.GetSnapshot(id, maxVersion);
            if (snapshot != null)
            {
                minVersion = snapshot.Version + 1;
            }
            var eventStream = eventStore.ReadFrom(id, minVersion, maxVersion);
            var aggregateRoot = (Questionnaire) domainRepository.Load(typeof (Questionnaire), snapshot, eventStream);

            if (!eventStream.IsEmpty)
                snapshotStore.SaveShapshot(new Snapshot(id, maxVersion, aggregateRoot.CreateSnapshot()));

            return aggregateRoot;
        }
    }
}