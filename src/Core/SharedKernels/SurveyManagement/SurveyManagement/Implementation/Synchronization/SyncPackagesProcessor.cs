using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public void ProcessNextSyncPackageBatchInParallel(int batchSize, int maxDegreeOfParallelism)
        {
            var logkey = DateTime.Now.ToString("s");

            Stopwatch stopwatch = Stopwatch.StartNew();

            IReadOnlyCollection<string> packagePathes = incomingSyncPackagesQueue.GetTopSyncItemsAsFileNames(batchSize);

            this.logger.Debug($"Sync process [{logkey}]: Received {packagePathes.Count} packages for procession. Took {stopwatch.Elapsed:g}.");
            stopwatch.Restart();

            if (packagePathes.Count > 0)
            {
                Parallel.ForEach(packagePathes.Select((path, index) => new { path, index }),
                   new ParallelOptions
                   {
                       MaxDegreeOfParallelism = maxDegreeOfParallelism
                   },
                   x => 
                   {
                       try
                       {
                           this.ProcessSinglePackage(x.path, $"#{x.index}_{logkey}");
                       }
                       catch (Exception e)
                       {
                           this.logger.Error($"Failed to process package {x.path}.", e);
                       }
                   });
            }

            this.logger.Info($"Sync process [{logkey}]: Processed {packagePathes.Count} packages. Took {stopwatch.Elapsed:g}.");
        }

        private void ProcessSinglePackage(string syncPackagePath, string logkey)
        {
            Stopwatch innerwatch = Stopwatch.StartNew();

            IncomingSyncPackage syncPackage = null;
            try
            {
                syncPackage = this.incomingSyncPackagesQueue.GetSyncItem(syncPackagePath);

                this.logger.Debug($"Sync process [{logkey}] package [{Path.GetFileName(syncPackagePath)}]: Read {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }
            catch (IncomingSyncPackageException e)
            {
                this.brokenSyncPackagesStorage.StoreUnhandledPackage(e.PathToPackage, e.InterviewId, e.InnerException);

                this.incomingSyncPackagesQueue.DeleteSyncItem(e.PathToPackage);
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
                this.commandService.Execute(command, syncPackage.Origin);

                this.logger.Debug($"Sync process [{logkey}] package [{Path.GetFileName(syncPackagePath)}]: Executed command for {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }
            catch (Exception e)
            {
                this.logger.Error($"Sync package '{syncPackage.PathToPackage}' wasn't processed. Reason: '{e.Message}'", e);
                this.brokenSyncPackagesStorage.StoreUnhandledPackage(syncPackage.PathToPackage, syncPackage.InterviewId, e);

                this.logger.Debug($"Sync process [{logkey}] package [{Path.GetFileName(syncPackagePath)}]: Failed to execute command for {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }

            this.incomingSyncPackagesQueue.DeleteSyncItem(syncPackage.PathToPackage);

            this.logger.Debug($"Sync process [{logkey}] package [{Path.GetFileName(syncPackagePath)}]: Deleted {Path.GetFileName(syncPackagePath)}. Took {innerwatch.Elapsed:g}.");
            innerwatch.Stop();
        }
    }
}
