using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Main.Core.Events;

namespace RavenQuestionnaire.Web.Synchronization
{
    public class HQEventSync : AbstractEventSync
    {
        private IEventSync synchronizer;
        public HQEventSync(IEventSync synchronizer)
        {
            this.synchronizer = synchronizer;
        }

        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            return this.synchronizer.ReadEvents();
        }

        #endregion
    }
}
