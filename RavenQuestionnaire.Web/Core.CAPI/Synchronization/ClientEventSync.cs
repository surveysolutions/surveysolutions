using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Denormalizers;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Ncqrs;
using Ncqrs.Eventing.Storage;

namespace Core.CAPI.Synchronization
{
    public class ClientEventSync : AbstractEventSync
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage;
        private readonly IEventStore myEventStore;

        public ClientEventSync(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage)
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

            List<AggregateRootEvent> retval = new List<AggregateRootEvent>();
            foreach (var item in storage.Query())
            {
                if (!SurveyStatus.IsStatusAllowCapiSync(item.Status))
                    continue;
                retval.AddRange(GetEventStreamById(item.CompleteQuestionnaireId));
            }
            // return retval;
            return retval.OrderBy(x => x.EventTimeStamp);
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
