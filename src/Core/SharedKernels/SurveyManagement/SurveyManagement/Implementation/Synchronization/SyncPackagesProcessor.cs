using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Quartz;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesProcessor : ISyncPackagesProcessor, IJob
    {
        private IIncomingPackagesQueue incomingPackagesQueue;
        private IUnhandledPackageStorage unhandledPackageStorage;
        private readonly string origin;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;
        private IStreamableEventStore eventStore;
        private IEventDispatcher eventBus;

        internal IStreamableEventStore EventStore
        {
            get { return eventStore ?? NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore; }
            set { eventStore = value; }
        }

        internal IEventDispatcher EventBus
        {
            get { return eventBus ?? NcqrsEnvironment.Get<IEventBus>() as IEventDispatcher; }
            set { eventBus = value; }
        }

        public SyncPackagesProcessor(ILogger logger, SyncSettings syncSettings, ICommandService commandService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, IArchiveUtils archiver,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter, IIncomingPackagesQueue incomingPackagesQueue, IUnhandledPackageStorage unhandledPackageStorage)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;
            this.incomingPackagesQueue = incomingPackagesQueue;
            this.unhandledPackageStorage = unhandledPackageStorage;
            origin = syncSettings.Origin;
        }

        public void ProcessNextSyncPackage()
        {
            string fileToProcess = incomingPackagesQueue.DeQueue();

            if (string.IsNullOrEmpty(fileToProcess) || !fileSystemAccessor.IsFileExists(fileToProcess))
                return;

            Guid? interviewId = null;
            try
            {
                var syncItem = jsonUtils.Deserialize<SyncItem>(fileSystemAccessor.ReadAllText(fileToProcess));

                interviewId = syncItem.RootId;

                var meta =
                    jsonUtils.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                var items =
                    jsonUtils.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content));

                if (IsNewInterivewCreatedOnClient(meta))
                {
                    AnsweredQuestionSynchronizationDto[] prefilledQuestions = null;
                    if (meta.FeaturedQuestionsMeta != null)
                    {
                        prefilledQuestions = meta.FeaturedQuestionsMeta
                            .Select(
                                q =>
                                    new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value,
                                        string.Empty))
                            .ToArray();
                    }

                    commandService.Execute(
                        new CreateInterviewCreatedOnClientCommand(
                            interviewId: meta.PublicKey,
                            userId: meta.ResponsibleId,
                            questionnaireId: meta.TemplateId,
                            questionnaireVersion: meta.TemplateVersion,
                            status: (InterviewStatus) meta.Status,
                            featuredQuestionsMeta: prefilledQuestions,
                            isValid: meta.Valid), origin);

                }
                else
                    commandService.Execute(
                        new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                            meta.TemplateVersion,
                            (InterviewStatus) meta.Status, null, meta.Comments, meta.Valid), origin);

                var incomeEvents = BuildEventStreams(items,
                    EventStore.GetLastEventSequence(syncItem.RootId));
                EventStore.Store(incomeEvents);

                EventBus.Publish(incomeEvents);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                unhandledPackageStorage.StoreUnhandledPackage(fileToProcess, interviewId);
            }

            fileSystemAccessor.DeleteFile(fileToProcess);
        }

        private bool IsNewInterivewCreatedOnClient(InterviewMetaInfo meta)
        {
            return meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value &&
                   interviewSummaryRepositoryWriter.GetById(meta.PublicKey) == null;
        }

        public void Execute(IJobExecutionContext context)
        {
            ProcessNextSyncPackage();
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
