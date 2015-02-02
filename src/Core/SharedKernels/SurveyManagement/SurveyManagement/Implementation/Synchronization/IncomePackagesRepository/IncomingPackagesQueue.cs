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
using Quartz;
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
    [DisallowConcurrentExecution]
    internal class IncomingPackagesQueue : IIncomingPackagesQueue, IJob
    {
        private string incomingCapiPackagesWithErrorsDirectory;
        private string incomingUnprocessedPackagesDirectory;
        private readonly string origin;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;
        private IStreamableEventStore eventStore;
        private IEventDispatcher eventBus;

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

        public IncomingPackagesQueue(ILogger logger, SyncSettings syncSettings, ICommandService commandService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, IArchiveUtils archiver,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter)
        {
            this.logger = logger;
            this.syncSettings = syncSettings;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;
            this.origin = syncSettings.Origin;

            this.InitializeDirectoriesForCapiIncomePackages();
        }

        public void DeQueue()
        {
            var fileToProcess =
                fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).FirstOrDefault();

            if (string.IsNullOrEmpty(fileToProcess) || !fileSystemAccessor.IsFileExists(fileToProcess))
                return;

            Guid? interviewId = null;
            try
            {
                var syncItem = this.jsonUtils.Deserialize<SyncItem>(fileSystemAccessor.ReadAllText(fileToProcess));
                
                interviewId = syncItem.RootId;
                
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
                    commandService.Execute(
                        new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                            meta.TemplateVersion,
                            (InterviewStatus) meta.Status, null, meta.Comments, meta.Valid, false), origin);


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
                if (interviewId.HasValue)
                    StoreErrorPackageAtInterviewCorrespondingFolder(interviewId.Value, fileToProcess);
                else
                    this.fileSystemAccessor.CopyFileOrDirectory(fileToProcess, incomingCapiPackagesWithErrorsDirectory);
                return;
            }

            this.fileSystemAccessor.DeleteFile(fileToProcess);
        }

        public string[] GetListOfUnhandledPackagesForInterview(Guid interviewId)
        {
             var interviewFolder = this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory,
               interviewId.FormatGuid());

            if (!this.fileSystemAccessor.IsDirectoryExists(interviewFolder))
                return new string[0];

            return fileSystemAccessor.GetFilesInDirectory(interviewFolder);
        }

        public void PushSyncItem(string item)
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

        public int QueueLength
        {
            get { return fileSystemAccessor.GetFilesInDirectory(incomingUnprocessedPackagesDirectory).Count(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            this.DeQueue();
        }

        public IEnumerable<string> GetListOfUnhandledPackages()
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesWithErrorsDirectory))
                return Enumerable.Empty<string>();

            var syncFiles = this.fileSystemAccessor.GetFilesInDirectory(this.incomingCapiPackagesWithErrorsDirectory,
                string.Format("*.{0}", this.syncSettings.IncomingCapiPackageFileNameExtension));

            var result = new List<string>();
            foreach (var syncFile in syncFiles)
            {
                result.Add(this.fileSystemAccessor.GetFileName(syncFile));
            }

            var interviewFolders = fileSystemAccessor.GetDirectoriesInDirectory(this.incomingCapiPackagesWithErrorsDirectory);
            foreach (var interviewFolder in interviewFolders)
            {
                var interviewSyncFiles = this.fileSystemAccessor.GetFilesInDirectory(interviewFolder,
                    string.Format("*.{0}", this.syncSettings.IncomingCapiPackageFileNameExtension));

                foreach (var interviewSyncFile in interviewSyncFiles)
                {
                    result.Add(fileSystemAccessor.CombinePath(this.fileSystemAccessor.GetFileName(interviewFolder), this.fileSystemAccessor.GetFileName(interviewSyncFile)));
                }
            }
            return result;
        }

        public string GetUnhandledPackagePath(string package)
        {
            return this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory, package);
        }

        private void StoreErrorPackageAtInterviewCorrespondingFolder(Guid interviewId, string fileToProcess)
        {
            var interviewFolder = this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory,
               interviewId.FormatGuid());

            if (!this.fileSystemAccessor.IsDirectoryExists(interviewFolder))
                this.fileSystemAccessor.CreateDirectory(interviewFolder);

            this.fileSystemAccessor.CopyFileOrDirectory(fileToProcess, interviewFolder);
        }

        private string GetItemFileNameForErrorStorage(Guid id, int version = 1)
        {
            var fileName = this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesWithErrorsDirectory,
                string.Format("{0}V-{1}.{2}", id, version, this.syncSettings.IncomingCapiPackageFileNameExtension));

            if (fileSystemAccessor.IsFileExists(fileName))
                return GetItemFileNameForErrorStorage(id, version + 1);

            return fileName;
        }

        private void InitializeDirectoriesForCapiIncomePackages()
        {
            this.incomingUnprocessedPackagesDirectory = this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                this.syncSettings.IncomingUnprocessedPackagesDirectoryName);

            this.incomingCapiPackagesWithErrorsDirectory = this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                this.syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingUnprocessedPackagesDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingUnprocessedPackagesDirectory);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesWithErrorsDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingCapiPackagesWithErrorsDirectory);
        }

        private UncommittedEventStream BuildEventStreams(IEnumerable<AggregateRootEvent> stream, long initialVersion)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid(), null);
            var eventSequence = initialVersion + 1;
            foreach (var aggregateRootEvent in stream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(eventSequence, initialVersion));
                eventSequence++;
            }
            return uncommitedStream;
        }
    }
}
