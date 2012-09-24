using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;

namespace Core.HQ.Synchronization
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
