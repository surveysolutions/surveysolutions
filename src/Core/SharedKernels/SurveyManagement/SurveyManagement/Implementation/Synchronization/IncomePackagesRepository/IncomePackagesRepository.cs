using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core;
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
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository
{
    internal class IncomePackagesRepository : IIncomePackagesRepository
    {
        private string incomingCapiPackagesDirectory;
        private string incomingCapiPackagesWithErrorsDirectory;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;

        public IncomePackagesRepository(ILogger logger, SyncSettings syncSettings, ICommandService commandService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter)
        {
            this.logger = logger;
            this.syncSettings = syncSettings;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
            this.interviewSummaryRepositoryWriter = interviewSummaryRepositoryWriter;

            this.InitializeDirectoriesForCapiIncomePackages();
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
                var meta = this.jsonUtils.Deserrialize<InterviewMetaInfo>(PackageHelper.DecompressString(item.MetaInfo));
                if (meta.CreatedOnClient.HasValue && meta.CreatedOnClient.Value && this.interviewSummaryRepositoryWriter.GetById(meta.PublicKey)==null)
                {
                    AnsweredQuestionSynchronizationDto[] prefilledQuestions = null;
                    if (meta.FeaturedQuestionsMeta != null)
                        prefilledQuestions = meta.FeaturedQuestionsMeta
                            .Select(q => new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value, string.Empty))
                            .ToArray();

                    this.commandService.Execute(new CreateInterviewCreatedOnClientCommand(interviewId: meta.PublicKey,
                        userId: meta.ResponsibleId, questionnaireId: meta.TemplateId,
                        questionnaireVersion: meta.TemplateVersion.Value, status: (InterviewStatus) meta.Status,
                        featuredQuestionsMeta: prefilledQuestions, isValid: meta.Valid));

                }
                else
                    this.commandService.Execute(new ApplySynchronizationMetadata(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        (InterviewStatus)meta.Status, null, meta.Comments, meta.Valid, false));

                this.fileSystemAccessor.WriteAllText(this.GetItemFileName(meta.PublicKey), item.Content);
            }
            catch (Exception ex)
            {
                this.logger.Error("error on handling incoming package,", ex);
                this.fileSystemAccessor.WriteAllText(this.GetItemFileNameForErrorStorage(item.Id),this.jsonUtils.GetItemAsContent(item));
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

            var items = this.jsonUtils.Deserrialize<AggregateRootEvent[]>(PackageHelper.DecompressString(fileContent));
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
                    this.commandService.Execute(new ReevaluateSynchronizedInterview(id));
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
