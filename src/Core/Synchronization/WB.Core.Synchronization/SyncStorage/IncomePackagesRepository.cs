using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using Newtonsoft.Json;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private readonly string path;
        private const string FolderName = "IncomingData";
        private const string ErrorFolderName = "IncomingDataWithErrors"; 
        private const string FileExtension = "sync";

        public IncomePackagesRepository(string folderPath)
        {
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var errorPath = Path.Combine(path, ErrorFolderName);
            if (!Directory.Exists(errorPath))
                Directory.CreateDirectory(errorPath);
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");

            try
            {
                var meta = GetContentAsItem<InterviewMetaInfo>(item.MetaInfo);

                NcqrsEnvironment.Get<ICommandService>()
                    .Execute(new ApplySynchronizationMetadata(
                        meta.PublicKey, meta.ResponsibleId, meta.TemplateId, (InterviewStatus) meta.Status, null, meta.Comments, meta.Valid));

                File.WriteAllText(GetItemFileName(meta.PublicKey), item.Content);
            }
            catch
            {
                File.WriteAllText(GetItemFileNameForErrorStorage(item.Id), JsonConvert.SerializeObject(item));
            }

        }


        private string GetItemFileName(Guid id)
        {
            return Path.Combine(path, string.Format("{0}.{1}", id, FileExtension));
        }

        private string GetItemFileNameForErrorStorage(Guid id)
        {
            return Path.Combine(path, ErrorFolderName, string.Format("{0}.{1}", id, FileExtension));
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

        public IEnumerable<Guid> GetListOfUnhandledPackages()
        {
            var errorPath = Path.Combine(path, ErrorFolderName);
            if(!Directory.Exists(errorPath))
                return Enumerable.Empty<Guid>();
            var syncFiles = Directory.GetFiles(errorPath, string.Format("*.{0}", FileExtension));
            var result = new List<Guid>();
            foreach (var syncFile in syncFiles)
            {
                Guid packageId;
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(syncFile), out packageId))
                    result.Add(packageId);
            }
            return result;
        }

        public string GetUnhandledPackagePath(Guid id)
        {
            return this.GetItemFileNameForErrorStorage(id);
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
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            var events = eventStore.ReadFrom(id, sequence + 1, long.MaxValue);
            var latestEventSequence = events.IsEmpty ? sequence : events.Last().EventSequence;
            var incomeEvents = this.BuildEventStreams(stream, latestEventSequence);
            
            if (!incomeEvents.Any())
                return;

            eventStore.Store(incomeEvents);

            using (new EventContext())
            {
                commandService.Execute(new ReevaluateSynchronizedInterview(id));
            }
        }

        protected UncommittedEventStream BuildEventStreams(IEnumerable<AggregateRootEvent> stream, long sequence)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid());
            var i = sequence + 1;
            foreach (var aggregateRootEvent in stream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(i, 0, DateTime.UtcNow));
                i++;
            }
            return uncommitedStream;
        }

    }
}
