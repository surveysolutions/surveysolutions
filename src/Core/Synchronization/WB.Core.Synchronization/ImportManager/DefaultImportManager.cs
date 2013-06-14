using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events;
using Newtonsoft.Json;
using SynchronizationMessages.Export;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.ImportManager
{
    public class DefaultImportManager:IImportManager
    {
        public void Import(List<string> zipData)
        {
            var events = GetEventStream(zipData);
            var processor = new SyncEventHandler();
            processor.Merge(events);
            processor.Commit();

        }

        protected IEnumerable<AggregateRootEvent> GetEventStream(List<string> zipData)
        {
            var events = new List<AggregateRootEvent>();
            foreach (var file in zipData)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                var result = JsonConvert.DeserializeObject<ZipFileData>(file, settings);
                events.AddRange(result.Events);
            }

            return events;
        }

    }

}
