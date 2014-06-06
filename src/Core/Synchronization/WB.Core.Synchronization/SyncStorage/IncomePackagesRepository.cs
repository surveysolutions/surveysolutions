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
        private readonly string path;
        private readonly IQueryableReadSideRepositoryWriter<UserDocument> userStorage;

        private readonly ILogger logger;
        private readonly IJsonUtils jsonUtils;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICommandService commandService;
        private readonly SyncSettings syncSettings;

        private const string FolderName = "IncomingData";
        private const string ErrorFolderName = "IncomingDataWithErrors";
        private const string FileExtension = "sync";

        public IncomePackagesRepository(string folderPath, IQueryableReadSideRepositoryWriter<UserDocument> userStorage, ILogger logger,
            SyncSettings syncSettings, ICommandService commandService, IJsonUtils jsonUtils, IFileSystemAccessor fileSystemAccessor)
        {
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
            var errorPath = fileSystemAccessor.CombinePath(this.path, ErrorFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(errorPath))
                fileSystemAccessor.CreateDirectory(errorPath);

            this.userStorage = userStorage;
            this.logger = logger;
            this.syncSettings = syncSettings;
            this.commandService = commandService;
            this.jsonUtils = jsonUtils;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void StoreIncomingItem(SyncItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Content))
                throw new ArgumentException("Sync Item is not set.");

            try
            {
                var meta = jsonUtils.Deserrialize<InterviewMetaInfo>(item.MetaInfo);
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

                fileSystemAccessor.WriteAllText(this.GetItemFileName(meta.PublicKey), item.Content);
            }
            catch (Exception ex)
            {
                this.logger.Error("error on handling incoming package,", ex);
                fileSystemAccessor.WriteAllText(this.GetItemFileNameForErrorStorage(item.Id),jsonUtils.GetItemAsContent(item));
            }
        }

        private string GetItemFileName(Guid id)
        {
            return fileSystemAccessor.CombinePath(this.path, string.Format("{0}.{1}", id, FileExtension));
        }

        private string GetItemFileNameForErrorStorage(Guid id)
        {
            return fileSystemAccessor.CombinePath(fileSystemAccessor.CombinePath(this.path, ErrorFolderName),
                string.Format("{0}.{1}", id, FileExtension));
        }

        public void ProcessItem(Guid id)
        {
            var fileName = this.GetItemFileName(id);

            if (!fileSystemAccessor.IsFileExists(fileName))
                return;

            var fileContent = fileSystemAccessor.ReadAllText(fileName);
            
            var items = jsonUtils.Deserrialize<AggregateRootEvent[]>(fileContent);
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
                fileSystemAccessor.DeleteFile(fileName);

                bus.Publish(incomeEvents);
                if (this.syncSettings.ReevaluateInterviewWhenSynchronized)
                {
                    commandService.Execute(new ReevaluateSynchronizedInterview(id));
                }
            }
            else
            {
                fileSystemAccessor.DeleteFile(fileName);
            }
        }

        public IEnumerable<Guid> GetListOfUnhandledPackages()
        {
            var errorPath = fileSystemAccessor.CombinePath(this.path, ErrorFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(errorPath))
                return Enumerable.Empty<Guid>();

            var syncFiles = fileSystemAccessor.GetFilesInDirectory(errorPath).Where(f => f.EndsWith("." + FileExtension));
            var result = new List<Guid>();
            foreach (var syncFile in syncFiles)
            {
                var fileName = fileSystemAccessor.GetFileName(syncFile);
                var fileNameWithoutExtension = fileName.Substring(0, fileName.IndexOf(".") + 1);
                Guid packageId;
                if (Guid.TryParse(fileNameWithoutExtension, out packageId))
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
