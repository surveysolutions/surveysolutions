using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository
{
    internal class IncomePackagesRepository : IIncomePackagesRepository, IInterviewDetailsDataProcessor
    {
        private string incomingCapiPackagesWithErrorsDirectory;
        private string incomingUnprocessedPackagesDirectory;
        private readonly object locker = new object();
        private readonly string origin;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;
        private readonly ILogger logger;
        private readonly bool overrideReceivedEventTimeStamp;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;
        private IStreamableEventStore eventStore;
        private IEventDispatcher eventBus;
        public IncomePackagesRepository(ILogger logger, SyncSettings syncSettings, ICommandService commandService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, IArchiveUtils archiver,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter, bool overrideReceivedEventTimeStamp, string origin)
        {
            this.logger = logger;
            this.syncSettings = syncSettings;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;
            this.overrideReceivedEventTimeStamp = overrideReceivedEventTimeStamp;
            this.origin = origin;

            this.InitializeDirectoriesForCapiIncomePackages();
        }

        internal IStreamableEventStore EventStore
        {
            get { return this.eventStore ?? NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore; }
            set { this.eventStore = value; }
        }

        internal IEventDispatcher EventBus
        {
            get { return this.eventBus ?? NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher; }
            set { this.eventBus = value; }
        }

        private void InitializeDirectoriesForCapiIncomePackages()
        {
            this.incomingUnprocessedPackagesDirectory= this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                this.syncSettings.IncomingUnprocessedPackagesDirectoryName);

            this.incomingCapiPackagesWithErrorsDirectory = this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                this.syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingUnprocessedPackagesDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingUnprocessedPackagesDirectory);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesWithErrorsDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingCapiPackagesWithErrorsDirectory);
        }

        public void Process()
        {
            lock (locker)
            {
                var fileToProcess =
                    fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).FirstOrDefault();

                if (string.IsNullOrEmpty(fileToProcess) || !fileSystemAccessor.IsFileExists(fileToProcess))
                    return;

                try
                {
                    var syncItem = this.jsonUtils.Deserialize<SyncItem>(fileSystemAccessor.ReadAllText(fileToProcess));

                    var meta =
                        this.jsonUtils.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                    if (meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value &&
                        this.interviewSummaryRepositoryWriter.GetById(meta.PublicKey) == null)
                    {
                        AnsweredQuestionSynchronizationDto[] prefilledQuestions = null;
                        if (meta.FeaturedQuestionsMeta != null)
                            prefilledQuestions = meta.FeaturedQuestionsMeta
                                .Select(
                                    q =>
                                        new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value,
                                            string.Empty))
                                .ToArray();

                        this.commandService.Execute(
                            new CreateInterviewCreatedOnClientCommand(interviewId: meta.PublicKey,
                                userId: meta.ResponsibleId, questionnaireId: meta.TemplateId,
                                questionnaireVersion: meta.TemplateVersion, status: (InterviewStatus) meta.Status,
                                featuredQuestionsMeta: prefilledQuestions, isValid: meta.Valid), origin);

                    }
                    else
                        commandService.Execute(new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                            meta.TemplateVersion,
                            (InterviewStatus)meta.Status, null, meta.Comments, meta.Valid, false), origin);


                    var items =
                        this.jsonUtils.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content));
                    if (items.Length > 0)
                    {
                        var incomeEvents = this.BuildEventStreams(items,
                            this.EventStore.GetLastEventSequence(syncItem.RootId));

                        this.EventStore.Store(incomeEvents);

                        this.EventBus.Publish(incomeEvents);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    this.fileSystemAccessor.CopyFileOrDirectory(fileToProcess, incomingCapiPackagesWithErrorsDirectory);
                    return;
                }

                this.fileSystemAccessor.DeleteFile(fileToProcess);
            }
        }

        public void StoreIncomingItem(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Sync Item is not set.");

            try
            {
                this.fileSystemAccessor.WriteAllText(
                    this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory,
                        string.Format("{0}.{1}", Guid.NewGuid().FormatGuid(),
                            this.syncSettings.IncomingCapiPackageFileNameExtension)), item);
            }
            catch (Exception ex)
            {
                this.logger.Error("error on handling incoming package,", ex);
                throw;
            }
        }

        private string GetItemFileNameForErrorStorage(Guid id, int version = 1)
        {
            var fileName = this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory,
                string.Format("{0}V-{1}.{2}", id,version, this.syncSettings.IncomingCapiPackageFileNameExtension));

            if (fileSystemAccessor.IsFileExists(fileName))
                return GetItemFileNameForErrorStorage(id, version + 1);
            
            return fileName;
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

        public int GetUnprocessedPackagesCount()
        {
            return fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).Count();
        }

        protected UncommittedEventStream BuildEventStreams(IEnumerable<AggregateRootEvent> stream, long initialVersion)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid(), null);
            var eventSequence = initialVersion + 1;
            foreach (var aggregateRootEvent in stream)
            {
                uncommitedStream.Append(this.overrideReceivedEventTimeStamp
                    ? aggregateRootEvent.CreateUncommitedEvent(eventSequence, initialVersion, DateTime.UtcNow)
                    : aggregateRootEvent.CreateUncommitedEvent(eventSequence, initialVersion));
                eventSequence++;
            }
            return uncommitedStream;
        }
    }
}
