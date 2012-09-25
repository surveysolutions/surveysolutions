using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;

namespace Core.HQ.Synchronization
{
    using System;

    using Main.Core.Denormalizers;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.CompleteQuestionnaire;

    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    public class HQEventSync : AbstractEventSync
    {

        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage;
        private readonly IEventStore myEventStore;

        public HQEventSync(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage)
        {
            this.storage = storage;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
        }

        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");

            var retval = new List<AggregateRootEvent>();
            //var events = this.myEventStore;
            //return events.Select(e => new AggregateRootEvent(e)).ToList();
            foreach (var item in storage.Query())
            {
                retval.AddRange(GetEventStreamById(item.CompleteQuestionnaireId));
            }
            return retval.OrderBy(x => x.EventSourceId);
        }

        protected List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId)
        {
            var events = this.myEventStore.ReadFrom(aggregateRootId,
                                                    int.MinValue, int.MaxValue);
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }
        #endregion
    }
}
