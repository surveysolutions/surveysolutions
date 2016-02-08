using System;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class SyncPackagesProcessor : ISyncPackagesProcessor
    {
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly IBrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private readonly ILogger logger;
        private readonly ICommandService commandService;

        public SyncPackagesProcessor(ILogger logger, 
            ICommandService commandService,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue, 
            IBrokenSyncPackagesStorage brokenSyncPackagesStorage)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.brokenSyncPackagesStorage = brokenSyncPackagesStorage;
        }

        public void ProcessNextSyncPackage()
        {
            IncomingSyncPackage syncPackage = null;
            try
            {
                syncPackage = incomingSyncPackagesQueue.DeQueue();
            }
            catch (IncomingSyncPackageException e)
            {
                brokenSyncPackagesStorage.StoreUnhandledPackage(e.PathToPackage, e.InterviewId, e.InnerException);

                incomingSyncPackagesQueue.DeleteSyncItem(e.PathToPackage);
            }

            if (syncPackage == null)
            {
                return;
            }

            try
            {
                var command = new SynchronizeInterviewEventsCommand(syncPackage.InterviewId,
                    syncPackage.ResponsibleId,
                    syncPackage.QuestionnaireId,
                    syncPackage.QuestionnaireVersion,
                    syncPackage.EventsToSynchronize,
                    syncPackage.InterviewStatus,
                    syncPackage.CreatedOnClient);
                commandService.Execute(command, syncPackage.Origin);
            }
            catch (Exception e)
            {
                logger.Error(
                    $"package '{syncPackage.PathToPackage}' wasn't processed. Reason: '{e.Message}'",
                    e);
                brokenSyncPackagesStorage.StoreUnhandledPackage(syncPackage.PathToPackage, syncPackage.InterviewId,
                    e);
            }

            incomingSyncPackagesQueue.DeleteSyncItem(syncPackage.PathToPackage);
        }
    }
}
