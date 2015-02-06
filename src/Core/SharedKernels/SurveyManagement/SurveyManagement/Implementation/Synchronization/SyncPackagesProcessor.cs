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
        private IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private IUnhandledPackageStorage unhandledPackageStorage;
        private readonly string origin;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IJsonUtils jsonUtils;
        private readonly IArchiveUtils archiver;

        public SyncPackagesProcessor(ILogger logger, SyncSettings syncSettings, ICommandService commandService,
            IFileSystemAccessor fileSystemAccessor, IJsonUtils jsonUtils, IArchiveUtils archiver, IIncomingSyncPackagesQueue incomingSyncPackagesQueue, IUnhandledPackageStorage unhandledPackageStorage)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.jsonUtils = jsonUtils;
            this.archiver = archiver;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.unhandledPackageStorage = unhandledPackageStorage;
            origin = syncSettings.Origin;
        }

        public void ProcessNextSyncPackage()
        {
            string fileToProcess = incomingSyncPackagesQueue.DeQueue();

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
                    jsonUtils.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content))
                        .Select(e => e.Payload)
                        .ToArray();

                commandService.Execute(
                    new SynchronizeInterviewEvents(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        meta.TemplateVersion, items, (InterviewStatus) meta.Status, meta.CreatedOnClient ?? false),
                    origin);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("package '{0}' wasn't processed. Reason: '{1}'", fileToProcess, e.Message), e);
                unhandledPackageStorage.StoreUnhandledPackage(fileToProcess, interviewId);
            }

            fileSystemAccessor.DeleteFile(fileToProcess);
        }

        public void Execute(IJobExecutionContext context)
        {
            ProcessNextSyncPackage();
        }
    }
}
