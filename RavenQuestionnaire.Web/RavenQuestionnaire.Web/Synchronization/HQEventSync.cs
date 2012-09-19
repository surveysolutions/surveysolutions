using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Main.Core.Events;
using Raven.Client.Document;

namespace RavenQuestionnaire.Web.Synchronization
{
    public class HQEventSync : AbstractEventSync
    {
     //   private DocumentStore store;
        public HQEventSync()
        {
           // this.store = store;
        }

        #region Overrides of AbstractEventSync

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
          /*  var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");*/
            return Enumerable.Empty<AggregateRootEvent>();
        }

        #endregion
    }
}
