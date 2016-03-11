using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Main.Core.Events;
using Polly;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    [Obsolete("Since v 5.8")]
    internal class IncomingSyncPackagesQueue : InterviewPackagesService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingUnprocessedPackagesDirectory;
        private readonly SyncSettings syncSettings;
        private readonly ILogger logger;
        private readonly ISerializer serializer;
        private readonly IArchiveUtils archiver;
        private readonly MemoryCache cache = new MemoryCache(nameof(IncomingSyncPackagesQueue));
        private readonly object cacheLockObject = new object();
        private readonly IBrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private readonly ICommandService commandService;
        
        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor,
            SyncSettings syncSettings,
            ILogger logger,
            ISerializer serializer,
            IArchiveUtils archiver,
            IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage,
            ICommandService commandService) : base(
                interviewPackageStorage: interviewPackageStorage,
                brokenInterviewPackageStorage: brokenInterviewPackageStorage,
                logger: logger,
                serializer: serializer,
                archiver: archiver,
                syncSettings: syncSettings,
                commandService: commandService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;
            this.logger = logger;
            this.serializer = serializer;
            this.archiver = archiver;
            this.brokenSyncPackagesStorage = brokenSyncPackagesStorage;
            this.commandService = commandService;
            this.incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);
        }

        public override int QueueLength => this.GetCachedFilesInIncomingDirectory()
            .Count(filename => filename.EndsWith(this.syncSettings.IncomingCapiPackageFileNameExtension)) + base.QueueLength;

        private ContextualPolicy SetupRetryPolicyForPackage(string pathToPackage)
        {
            return Policy
                .Handle<Win32Exception>()
                .WaitAndRetry(
                    syncSettings.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(syncSettings.RetryIntervalInSeconds),
                    (exception, retryCount, context) =>
                    {
                        this.logger.Warn($"Sync package '{pathToPackage}' failed to open with error '{exception.Message}'", exception);
                    }
                );
        }

        public override bool HasPackagesByInterviewId(Guid interviewId)
        {
            return base.HasPackagesByInterviewId(interviewId) ||
                   this.GetCachedFilesInIncomingDirectory()
                       .Any(filename => filename.Contains(interviewId.FormatGuid()) &&
                                        filename.EndsWith(this.syncSettings.IncomingCapiPackageFileNameExtension));
        }

        private ConcurrentHashSet<string> GetCachedFilesInIncomingDirectory()
        {
            object cachedFileNames = this.cache.Get("incomingPackagesFileNames");

            if (cachedFileNames == null)
            {
                lock (this.cacheLockObject)
                {
                    cachedFileNames = this.cache.Get("incomingPackagesFileNames");

                    if (cachedFileNames == null)
                    {
                        var filenames = new ConcurrentHashSet<string>(this.GetFilesInIncomingDirectoryImpl().ToList());

                        this.cache.Set("incomingPackagesFileNames", filenames, DateTime.Now.AddMinutes(5));

                        cachedFileNames = filenames;
                    }
                }
            }

            return (ConcurrentHashSet<string>) cachedFileNames;
        }

        private IEnumerable<string> GetFilesInIncomingDirectoryImpl()
        {
            foreach (string filename in this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory))
            {
                yield return filename;
            }

            foreach (string subfolder in this.fileSystemAccessor.GetDirectoriesInDirectory(this.incomingUnprocessedPackagesDirectory))
            {
                foreach (string filename in this.fileSystemAccessor.GetFilesInDirectory(subfolder))
                {
                    yield return filename;
                }
            }
        }

        public override IReadOnlyCollection<string> GetTopSyncItemsAsFileNames(int count)
        {
            var cachedFilesInIncomingDirectory = this.GetCachedFilesInIncomingDirectory().ToList();
            this.logger.Debug($"Current queue length: {cachedFilesInIncomingDirectory.Count}");

            var packagesFromFileStorage = cachedFilesInIncomingDirectory.OrderBy(this.fileSystemAccessor.GetFileName).Take(count).ToList();

            return packagesFromFileStorage.Count == 0 ? base.GetTopSyncItemsAsFileNames(count) : packagesFromFileStorage;
        }

        public override void ProcessPackage(string pathToPackage)
        {
            Guid? interviewId = null;
            string fileContent = null;
            try
            {
                if (!this.fileSystemAccessor.IsFileExists(pathToPackage))
                {
                    base.ProcessPackage(pathToPackage);
                    return;
                }

                var policy = SetupRetryPolicyForPackage(pathToPackage);


                Stopwatch innerwatch = Stopwatch.StartNew();
                fileContent = policy.Execute(() => fileSystemAccessor.ReadAllText(pathToPackage));
                this.logger.Debug($"GetSyncItem {Path.GetFileName(pathToPackage)}: ReadAllText {Path.GetFileName(pathToPackage)}. Took {innerwatch.Elapsed:g}.");
                innerwatch.Stop();

                var syncItem = this.serializer.Deserialize<SyncItem>(fileContent);

                interviewId = syncItem.RootId;

                var meta = this.serializer.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                var eventsToSync = this.serializer
                    .Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content))
                    .Select(e => e.Payload)
                    .ToArray();

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: meta.PublicKey,
                    userId: meta.ResponsibleId,
                    questionnaireId: meta.TemplateId,
                    questionnaireVersion: meta.TemplateVersion,
                    interviewStatus: (InterviewStatus) meta.Status,
                    createdOnClient: meta.CreatedOnClient ?? false,
                    synchronizedEvents: eventsToSync), syncSettings.Origin);
            }
            catch (Exception e)
            {
                this.logger.Error($"package '{pathToPackage}' wasn't parsed. Reason: '{e.Message}'", e);

                if (interviewId.HasValue && !string.IsNullOrEmpty(fileContent))
                    base.Enqueue(interviewId.Value, fileContent);
                else
                    this.brokenSyncPackagesStorage.StoreUnhandledPackage(pathToPackage, interviewId, e);
            }
            finally
            {
                this.fileSystemAccessor.DeleteFile(pathToPackage);
                this.GetCachedFilesInIncomingDirectory().Remove(pathToPackage);
            }
        }
    }
}