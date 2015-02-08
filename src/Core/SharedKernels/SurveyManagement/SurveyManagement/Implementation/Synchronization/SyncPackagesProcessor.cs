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
        private readonly ILogger logger;
        private readonly ICommandService commandService;

        public SyncPackagesProcessor(ILogger logger, ICommandService commandService,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, IUnhandledPackageStorage unhandledPackageStorage)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.unhandledPackageStorage = unhandledPackageStorage;
        }

        public void ProcessNextSyncPackage()
        {
            IncomingSyncPackages incomingSyncPackages = incomingSyncPackagesQueue.DeQueue();

            if (incomingSyncPackages==null)
                return;

            try
            {
                commandService.Execute(
                    new SynchronizeInterviewEvents(incomingSyncPackages.InterviewId, incomingSyncPackages.ResponsibleId, incomingSyncPackages.QuestionnaireId,
                        incomingSyncPackages.QuestionnaireVersion, incomingSyncPackages.EventsToSynchronize, incomingSyncPackages.InterviewStatus, incomingSyncPackages.CreatedOnClient),
                    incomingSyncPackages.Origin);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("package '{0}' wasn't processed. Reason: '{1}'", incomingSyncPackages.PathToPackage, e.Message), e);
                unhandledPackageStorage.StoreUnhandledPackage(incomingSyncPackages.PathToPackage, incomingSyncPackages.InterviewId);
            }

            incomingSyncPackagesQueue.DeleteSyncItem(incomingSyncPackages.PathToPackage);
        }

        public void Execute(IJobExecutionContext context)
        {
            ProcessNextSyncPackage();
        }
    }
}
