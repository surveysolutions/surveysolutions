using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.Synchronization.SyncStorage
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private string incomingCapiPackagesDirectory;
        private string incomingCapiPackagesWithErrorsDirectory;
        private readonly IQueryableReadSideRepositoryWriter<UserDocument> userStorage;

        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public IncomePackagesRepository(IQueryableReadSideRepositoryWriter<UserDocument> userStorage, ILogger logger,
            SyncSettings syncSettings, ICommandService commandService, IFileSystemAccessor fileSystemAccessor)
        {
            this.userStorage = userStorage;
            this.logger = logger;
            this.syncSettings = syncSettings;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;

            InitializeDirectoriesForCapiIncomePackages();
        }

        private void InitializeDirectoriesForCapiIncomePackages()
        {
            this.incomingCapiPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingCapiPackagesDirectoryName);

            this.incomingCapiPackagesWithErrorsDirectory = fileSystemAccessor.CombinePath(this.incomingCapiPackagesDirectory,
                this.syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingCapiPackagesDirectory);

            if (!this.fileSystemAccessor.IsDirectoryExists(incomingCapiPackagesWithErrorsDirectory))
                this.fileSystemAccessor.CreateDirectory(incomingCapiPackagesWithErrorsDirectory);
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");

            try
            { 
                var meta = jsonUtils.Deserrialize<InterviewMetaInfo>(PackageHelper.DecompressString(item.MetaInfo));
                if (meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value)
                {
                    AnsweredQuestionSynchronizationDto[] prefilledQuestions = null;
                    if (meta.FeaturedQuestionsMeta != null)
                        prefilledQuestions = meta.FeaturedQuestionsMeta
                            .Select(q => new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value, string.Empty))
                            .ToArray();

                    commandService.Execute(new CreateInterviewCreatedOnClientCommand(interviewId: meta.PublicKey,
                        userId: meta.ResponsibleId, questionnaireId: meta.TemplateId,
                        questionnaireVersion: meta.TemplateVersion.Value, status: (InterviewStatus) meta.Status,
                        featuredQuestionsMeta: prefilledQuestions, comments: meta.Comments));

                }
                else
                    commandService.Execute(new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        (InterviewStatus)meta.Status, null, meta.Comments, meta.Valid, false));

                this.fileSystemAccessor.WriteAllText(this.GetItemFileName(meta.PublicKey), item.Content);
            }
            catch (Exception ex)
            {
                this.logger.Error("error on handling incoming package,", ex);
                fileSystemAccessor.WriteAllText(this.GetItemFileNameForErrorStorage(item.Id),jsonUtils.GetItemAsContent(item));
            }
        }

        private string GetItemFileName(Guid id)
        {
            return this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesDirectory,
                string.Format("{0}.{1}", id, this.syncSettings.IncomingCapiPackageFileNameExtension));
        }

        private string GetItemFileNameForErrorStorage(Guid id)
        {
            return this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory,
                string.Format("{0}.{1}", id, this.syncSettings.IncomingCapiPackageFileNameExtension));
        }

        public void ProcessItem(Guid id)
        {
            var fileName = this.GetItemFileName(id);
            if (!this.fileSystemAccessor.IsFileExists(fileName))
                return;

            var fileContent = this.fileSystemAccessor.ReadAllText(fileName);

            var items = this.GetContentAsItem<AggregateRootEvent[]>(fileContent);
            if (items.Length > 0)
            {
                var eventStore = NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore;
                if (eventStore == null)
                    return;

                var bus = NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher;
                if (bus == null)
                    return;

                var incomeEvents = this.BuildEventStreams(items, eventStore.GetLastEventSequence(id));

                eventStore.Store(incomeEvents);
                this.fileSystemAccessor.DeleteFile(fileName);

                bus.Publish(incomeEvents);
                if (this.syncSettings.ReevaluateInterviewWhenSynchronized)
                {
                    commandService.Execute(new ReevaluateSynchronizedInterview(id));
                }
            }
            else
            {
                this.fileSystemAccessor.DeleteFile(fileName);
            }
        }

        public IEnumerable<Guid> GetListOfUnhandledPackages()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesWithErrorsDirectory))
                return Enumerable.Empty<Guid>();

            var syncFiles = this.fileSystemAccessor.GetFilesInDirectory(this.incomingCapiPackagesWithErrorsDirectory,
                string.Format("*.{0}", this.syncSettings.IncomingCapiPackageFileNameExtension));

            var result = new List<Guid>();
            foreach (var syncFile in syncFiles)
            {
                Guid packageId;
                if (Guid.TryParse(this.fileSystemAccessor.GetFileNameWithoutExtension(syncFile), out packageId))
                    result.Add(packageId);
            }
            return result;
        }

        public string GetUnhandledPackagePath(Guid id)
        {
            return this.GetItemFileNameForErrorStorage(id);
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
