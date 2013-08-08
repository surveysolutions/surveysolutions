using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Events;
using Main.Core.Utility;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private readonly string path;
        private const string FolderName = "IncomigData";
        private const string FileExtension = "sync";

        public IncomePackagesRepository(string folderPath)
        {
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");


            var meta = GetContentAsItem<InterviewMetaInfo>(item.MetaInfo);

            File.WriteAllText(GetItemFileName(meta.PublicKey), item.Content);


            NcqrsEnvironment.Get<ICommandService>()
                            .Execute(new UpdateInterviewMetaInfoCommand(meta.PublicKey, meta.TemplateId, meta.Title,
                                                                        meta.ResponsibleId, meta.Status.Id,
                                                                        null));
        }

        private string GetItemFileName(Guid id)
        {
            return Path.Combine(path, string.Format("{0}.{1}", id, FileExtension));
        }

        public void ProcessItem(Guid id, long sequence)
        {
            var fileName = GetItemFileName(id);
            if (!File.Exists(fileName))
                return;

            var fileContent = File.ReadAllText(fileName);

            var items = GetContentAsItem<AggregateRootEvent[]>(fileContent);

            StoreEvents(id, items, sequence);

            File.Delete(fileName);
        }

        private T GetContentAsItem<T>(string syncItemContent)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var item = JsonConvert.DeserializeObject<T>(PackageHelper.DecompressString(syncItemContent),
                settings);

            return item;
        }

        private void StoreEvents(Guid id, IEnumerable<AggregateRootEvent> stream, long sequence)
        {
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            var events = eventStore.ReadFrom(id, sequence + 1, long.MaxValue);
            var latestEventSequence = events.IsEmpty ? sequence : events.Last().EventSequence;
            var incomeEvents = this.BuildEventStreams(stream, latestEventSequence);
            eventStore.Store(incomeEvents);
        }

        protected UncommittedEventStream BuildEventStreams(IEnumerable<AggregateRootEvent> stream, long sequence)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid());
            var i = sequence + 1;
            foreach (var aggregateRootEvent in stream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(i,0));
                i++;
            }
            return uncommitedStream;
        }
    }
}
