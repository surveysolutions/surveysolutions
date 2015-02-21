using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.Implementation.ImportManager
{
    public class DefaultBackupManager : IBackupManager
    {
        private readonly IStreamableEventStore eventStore;
        private readonly ISyncEventHandler eventProcessor;

        public DefaultBackupManager()
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore;
            this.eventProcessor = new SyncEventHandler();
        }

        public ZipFileData Backup()
        {
            if (eventStore == null)
                return null;
            var retval = new ZipFileData();
            retval.Events =
                eventStore.GetAllEvents().Select(e => new AggregateRootEvent(e));
            return retval;
        }

        public void Restore(List<string> zipData)
        {
#warning restore must clean up db before updloading data
            var events = GetEventStream(zipData);

            eventProcessor.Process(events);
            
        }

        protected IEnumerable<AggregateRootEvent> GetEventStream(List<string> zipData)
        {
            var events = new List<AggregateRootEvent>();
            foreach (var file in zipData)
            {
                var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
                var result = JsonConvert.DeserializeObject<ZipFileData>(file, settings);
                events.AddRange(result.Events);
            }

            return events;
        }
    }
}
