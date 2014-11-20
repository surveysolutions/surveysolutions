﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private string incomingCapiPackagesDirectory;
        private string incomingCapiPackagesWithErrorsDirectory;

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
            this.incomingCapiPackagesDirectory = this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                this.syncSettings.IncomingCapiPackagesDirectoryName);

            this.incomingCapiPackagesWithErrorsDirectory = this.fileSystemAccessor.CombinePath(this.incomingCapiPackagesDirectory,
                this.syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingCapiPackagesDirectory);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.incomingCapiPackagesWithErrorsDirectory))
                this.fileSystemAccessor.CreateDirectory(this.incomingCapiPackagesWithErrorsDirectory);
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");

            try
            { 
                var meta = this.jsonUtils.Deserrialize<InterviewMetaInfo>(archiver.DecompressString(item.MetaInfo));
                if (meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value && this.interviewSummaryRepositoryWriter.GetById(meta.PublicKey)==null)
                {
                    AnsweredQuestionSynchronizationDto[] prefilledQuestions = null;
                    if (meta.FeaturedQuestionsMeta != null)
                        prefilledQuestions = meta.FeaturedQuestionsMeta
                            .Select(q => new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value, string.Empty))
                            .ToArray();

                    this.commandService.Execute(new CreateInterviewCreatedOnClientCommand(interviewId: meta.PublicKey,
                        userId: meta.ResponsibleId, questionnaireId: meta.TemplateId,
                        questionnaireVersion: meta.TemplateVersion, status: (InterviewStatus) meta.Status,
                        featuredQuestionsMeta: prefilledQuestions, isValid: meta.Valid), origin);

                }
                else
                    commandService.Execute(new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        meta.TemplateVersion,
                        (InterviewStatus)meta.Status, null, meta.Comments, meta.Valid, false), origin);

                this.fileSystemAccessor.WriteAllText(this.GetItemFileName(meta.PublicKey), item.Content);
            }
            catch (Exception ex)
            {
                this.logger.Error("error on handling incoming package,", ex);
                this.fileSystemAccessor.WriteAllText(this.GetItemFileNameForErrorStorage(item.Id),this.jsonUtils.GetItemAsContent(item));
            }
        }

        private bool IsInterviewPresent(Guid interviewId)
        {
            var interviewSummary = this.interviewSummaryRepositoryWriter.GetById(interviewId);
            if (interviewSummary == null)
                return false;
            return !interviewSummary.IsDeleted;
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

            if (!IsInterviewPresent(id))
            {
                this.fileSystemAccessor.WriteAllText(this.GetItemFileNameForErrorStorage(id), this.fileSystemAccessor.ReadAllText(fileName));
                this.fileSystemAccessor.DeleteFile(fileName);
                return;
            }

            var fileContent = this.fileSystemAccessor.ReadAllText(fileName);

            var items = this.jsonUtils.Deserrialize<AggregateRootEvent[]>(archiver.DecompressString(fileContent));
            if (items.Length > 0)
            {
                if (this.EventStore == null)
                    return;

                if (this.EventBus == null)
                    return;

                var incomeEvents = this.BuildEventStreams(items, this.EventStore.GetLastEventSequence(id));

                this.EventStore.Store(incomeEvents);
                this.fileSystemAccessor.DeleteFile(fileName);

                this.EventBus.Publish(incomeEvents);
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
                uncommitedStream.Append(this.overrideReceivedEventTimeStamp
                    ? aggregateRootEvent.CreateUncommitedEvent(i, 0, DateTime.UtcNow)
                    : aggregateRootEvent.CreateUncommitedEvent(i, 0));
                i++;
            }
            return uncommitedStream;
        }
    }
}
