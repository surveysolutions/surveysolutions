using System;
using Quartz;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    [DisallowConcurrentExecution]
    internal class SyncPackagesProcessor : ISyncPackagesProcessor, IJob
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
                if (e.InterviewId.HasValue)
                    brokenSyncPackagesStorage.StoreUnhandledPackageForInterview(e.PathToPackage, e.InterviewId.Value,
                        e.InnerException);
                else
                    brokenSyncPackagesStorage.StoreUnknownUnhandledPackage(e.PathToPackage, e.InnerException);

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
                logger.Error(string.Format("package '{0}' wasn't processed. Reason: '{1}'", syncPackage.PathToPackage, e.Message), e);

                var interviewException = e as InterviewException;
                if (interviewException != null)
                    brokenSyncPackagesStorage.StoreUnhandledPackageForInterviewInTypedFolder(syncPackage.PathToPackage, syncPackage.InterviewId, e, interviewException.ErrorType.ToString());
                else
                    brokenSyncPackagesStorage.StoreUnhandledPackageForInterview(syncPackage.PathToPackage, syncPackage.InterviewId, e);
            }

            incomingSyncPackagesQueue.DeleteSyncItem(syncPackage.PathToPackage);
        }

        public void Execute(IJobExecutionContext context)
        {
            ProcessNextSyncPackage();
        }
    }
}
