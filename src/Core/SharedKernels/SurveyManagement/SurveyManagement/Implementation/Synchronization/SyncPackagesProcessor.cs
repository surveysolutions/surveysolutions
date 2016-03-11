using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

        public void ProcessNextSyncPackageBatchInParallel(int batchSize, int MaxDegreeOfParallelism = 1)
        {

            var logkey = DateTime.Now.ToString("s");

            Stopwatch stopwatch = Stopwatch.StartNew();

            var packagePathes = incomingSyncPackagesQueue.DeQueueMany(batchSize);

            this.logger.Info($"Sync_{logkey}: Received {packagePathes.Count} packages for procession. Took {stopwatch.Elapsed:g}.");
            stopwatch.Restart();

            if (packagePathes.Count > 0)
            {
                Parallel.ForEach(packagePathes,
                   new ParallelOptions
                   {
                       MaxDegreeOfParallelism = MaxDegreeOfParallelism
                   },
                   syncPackagePath => {
                       Stopwatch innerwatch = Stopwatch.StartNew();

                       IncomingSyncPackage syncPackage = null;
                       try
                       {
                           syncPackage = this.incomingSyncPackagesQueue.GetSyncItem(syncPackagePath);

                           this.logger.Info($"Sync_inner_{logkey}: Read {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                           innerwatch.Restart();
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

                           this.logger.Info($"Sync_inner_{logkey}: Executed command for {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                           innerwatch.Restart();
                       }
                       catch (Exception e)
                       {
                           logger.Error(
                               $"package '{syncPackage.PathToPackage}' wasn't processed. Reason: '{e.Message}'",
                               e);
                           brokenSyncPackagesStorage.StoreUnhandledPackage(syncPackage.PathToPackage, syncPackage.InterviewId,
                               e);

                           this.logger.Info($"Sync_inner_{logkey}: Failed executed command for {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                           innerwatch.Restart();
                       }

                       incomingSyncPackagesQueue.DeleteSyncItem(syncPackage.PathToPackage);

                       this.logger.Info($"Sync_inner_{logkey}: Deleted {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                       innerwatch.Stop();
                   });
            }

            this.logger.Info($"Sync_{logkey}: Processed {packagePathes.Count} packages. Took {stopwatch.Elapsed:g}.");
            stopwatch.Stop();
        }
    }
}
