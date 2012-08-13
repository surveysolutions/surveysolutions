using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RavenQuestionnaire.Core.Events;
using SynchronizationMessages.CompleteQuestionnaire;

namespace Web.Supervisor.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetEventStreamService" in code, svc and config file together.
    public class GetEventStreamService : IGetEventStream
    {
        public GetEventStreamService(IEventSync eventStore)
        {
            this.eventStore = eventStore;
        }
        private IEventSync eventStore;
        #region Implementation of IGetEventStream

        public ImportSynchronizationMessage Process(Guid firstEventPulicKey, int length)
        {
            var stream = this.eventStore.ReadEvents().SkipWhile(e => e.EventIdentifier != firstEventPulicKey).Take(length).ToArray();
            //var index=stream
            return new ImportSynchronizationMessage() {EventStream = stream};
        }

        #endregion
    }
}
