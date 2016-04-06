using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Polly;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
        private readonly ICommandService commandService;
        
        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor,
            SyncSettings syncSettings,
            ILogger logger,
            ISerializer serializer,
            IArchiveUtils archiver,
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
            this.commandService = commandService;
            this.incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);
        }

        [Obsolete("Since v 5.8")]
        public override void StorePackage(string item)
        {
            if (string.IsNullOrEmpty(item)) throw new ArgumentException(nameof(item));

            var syncItem = this.serializer.Deserialize<SyncItem>(item);

            if (this.archiver.IsZipStream(new MemoryStream(Encoding.UTF8.GetBytes(syncItem.MetaInfo ?? ""))))
                syncItem.MetaInfo = archiver.DecompressString(syncItem.MetaInfo);

            if (this.archiver.IsZipStream(new MemoryStream(Encoding.UTF8.GetBytes(syncItem.Content ?? ""))))
                syncItem.Content = this.archiver.DecompressString(syncItem.Content);

            InterviewMetaInfo meta = this.serializer.Deserialize<InterviewMetaInfo>(syncItem.MetaInfo) ??
                                     new InterviewMetaInfo();

            base.StorePackage(
                interviewId: syncItem.RootId,
                questionnaireId: meta.TemplateId,
                questionnaireVersion: meta.TemplateVersion,
                responsibleId: meta.ResponsibleId,
                interviewStatus: (InterviewStatus) meta.Status,
                isCensusInterview: meta.CreatedOnClient ?? false,
                events: syncItem.Content ?? "");
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

        public override IReadOnlyCollection<string> GetTopPackageIds(int count)
        {
            var cachedFilesInIncomingDirectory = this.GetCachedFilesInIncomingDirectory().ToList();
            this.logger.Debug($"Current queue length: {cachedFilesInIncomingDirectory.Count}");

            var packagesFromFileStorage = cachedFilesInIncomingDirectory.OrderBy(this.fileSystemAccessor.GetFileName).Take(count).ToList();

            return packagesFromFileStorage.Count == 0 ? base.GetTopPackageIds(count) : packagesFromFileStorage;
        }

        public override void ProcessPackage(string pathToPackage)
        {
            Stopwatch innerwatch = Stopwatch.StartNew();
            try
            {
                if (!this.fileSystemAccessor.IsFileExists(pathToPackage))
                {
                    base.ProcessPackage(pathToPackage);
                    return;
                }

                var policy = SetupRetryPolicyForPackage(pathToPackage);
                
                string fileContent = policy.Execute(() => fileSystemAccessor.ReadAllText(pathToPackage));

                this.logger.Debug($"Package {Path.GetFileName(pathToPackage)}. Read content from file. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                this.StorePackage(fileContent);

                this.fileSystemAccessor.DeleteFile(pathToPackage);
                this.GetCachedFilesInIncomingDirectory().Remove(pathToPackage);

                this.logger.Debug($"Package {Path.GetFileName(pathToPackage)}. File deleted. Took {innerwatch.Elapsed:g}.");
                innerwatch.Stop();
            }
            catch (Exception e)
            {
                this.logger.Error($"Package {Path.GetFileName(pathToPackage)}. FAILED. Reason: '{e.Message}'", e);
            }
        }
    }
}