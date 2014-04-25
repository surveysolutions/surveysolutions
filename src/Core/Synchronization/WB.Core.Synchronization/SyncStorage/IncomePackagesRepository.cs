﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private readonly string path;
        private readonly IQueryableReadSideRepositoryWriter<UserDocument> userStorage;

        private readonly ILogger logger;
        private readonly SyncSettings syncSettings;

        private const string FolderName = "IncomingData";
        private const string ErrorFolderName = "IncomingDataWithErrors"; 
        private const string FileExtension = "sync";

        public IncomePackagesRepository(string folderPath, IQueryableReadSideRepositoryWriter<UserDocument> userStorage, ILogger logger, SyncSettings syncSettings)
        {
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var errorPath = Path.Combine(path, ErrorFolderName);
            if (!Directory.Exists(errorPath))
                Directory.CreateDirectory(errorPath);

            this.userStorage = userStorage;
            this.logger = logger;
            this.syncSettings = syncSettings;
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");

            try
            {
                var meta = GetContentAsItem<InterviewMetaInfo>(item.MetaInfo);

                var user = userStorage.Query(_ => _.Where(u => u.PublicKey == meta.ResponsibleId).ToList().FirstOrDefault());

                Guid supervisorId = user.Supervisor.Id;

                if (meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value)
                {
                    NcqrsEnvironment.Get<ICommandService>()
                        .Execute(new CreateInterviewOnClientCommand(meta.PublicKey, meta.ResponsibleId, meta.TemplateId, meta.TemplateVersion,
                            DateTime.UtcNow, supervisorId));
                }

                NcqrsEnvironment.Get<ICommandService>()
                    .Execute(new ApplySynchronizationMetadata(
                        meta.PublicKey, meta.ResponsibleId, meta.TemplateId, (InterviewStatus) meta.Status, null, meta.Comments, meta.Valid));

                File.WriteAllText(GetItemFileName(meta.PublicKey), item.Content);
            }
            catch (Exception ex)
            {
                logger.Error("error on handling incoming package,", ex);

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

        public void ProcessItem(Guid id)
        {
            var fileName = GetItemFileName(id);
            if (!File.Exists(fileName))
                return;

            var fileContent = File.ReadAllText(fileName);

            var items = this.GetContentAsItem<AggregateRootEvent[]>(fileContent);
            if (items.Length > 0)
            {
                var eventStore = NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore;
                if (eventStore == null)
                    return;

                var bus = NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher;
                if (bus == null)
                    return;
                
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                if (commandService == null)
                    return;

                var incomeEvents = this.BuildEventStreams(items, eventStore.GetLastEventSequence(id));

                eventStore.Store(incomeEvents);
                File.Delete(fileName);

                bus.Publish(incomeEvents);
                if (this.syncSettings.ReevaluateInterviewWhenSynchronized)
                {
                    commandService.Execute(new ReevaluateSynchronizedInterview(id));
                }
            }
            else
            {
                File.Delete(fileName);
            }
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

        protected UncommittedEventStream BuildEventStreams(IEnumerable<AggregateRootEvent> stream, long sequence)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid(), null);
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
