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
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class IncomingSyncPackagesQueue : IIncomingSyncPackagesQueue
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingUnprocessedPackagesDirectory;
        private readonly SyncSettings syncSettings;
        private readonly ILogger logger;
        private readonly ISerializer serializer;
        private readonly IArchiveUtils archiver;
        private readonly MemoryCache cache = new MemoryCache(nameof(IncomingSyncPackagesQueue));
        private readonly object cacheLockObject = new object();

        public IncomingSyncPackagesQueue(IFileSystemAccessor fileSystemAccessor, 
            SyncSettings syncSettings, 
            ILogger logger, 
            ISerializer serializer, 
            IArchiveUtils archiver)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;
            this.logger = logger;
            this.serializer = serializer;
            this.archiver = archiver;
            this.incomingUnprocessedPackagesDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingUnprocessedPackagesDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(this.incomingUnprocessedPackagesDirectory))
                fileSystemAccessor.CreateDirectory(this.incomingUnprocessedPackagesDirectory);
        }

        public void Enqueue(Guid interviewId, string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("Sync Item is not set.");

            string syncPackageFileName = $"{DateTime.Now.Ticks}-{interviewId.FormatGuid()}.{this.syncSettings.IncomingCapiPackageFileNameExtension}";

            string subfolderName = $"{interviewId.FormatGuid()}".Substring(0, 2);
            string subfolderPath = this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory, subfolderName);

            if (!fileSystemAccessor.IsDirectoryExists(subfolderPath))
                fileSystemAccessor.CreateDirectory(subfolderPath);

            string fullPathToSyncPackage = this.fileSystemAccessor.CombinePath(subfolderPath, syncPackageFileName);

            Stopwatch innerwatch = Stopwatch.StartNew();
            this.fileSystemAccessor.WriteAllText(fullPathToSyncPackage, item);
            this.logger.Debug($"Sync package {syncPackageFileName}: WriteAllText {syncPackageFileName}. Took {innerwatch.Elapsed:g}.");
            innerwatch.Stop();

            this.GetCachedFilesInIncomingDirectory().Add(fullPathToSyncPackage);
        }

        public int QueueLength =>
            this.GetCachedFilesInIncomingDirectory()
                .Count(filename => filename.EndsWith(this.syncSettings.IncomingCapiPackageFileNameExtension));

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

        public void DeleteSyncItem(string syncItemPath)
        {
            fileSystemAccessor.DeleteFile(syncItemPath);

            this.GetCachedFilesInIncomingDirectory().Remove(syncItemPath);
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

        public bool HasPackagesByInterviewId(Guid interviewId)
        {
            return
                this.GetCachedFilesInIncomingDirectory().Any(filename =>
                    filename.Contains(interviewId.FormatGuid()) &&
                    filename.EndsWith(this.syncSettings.IncomingCapiPackageFileNameExtension));
        }

        public IReadOnlyCollection<string> GetTopSyncItemsAsFileNames(int count)
        {
            var cachedFilesInIncomingDirectory = this.GetCachedFilesInIncomingDirectory().ToList();
            this.logger.Debug($"Current queue length: {cachedFilesInIncomingDirectory.Count}");

            var pathToPackage = cachedFilesInIncomingDirectory.OrderBy(this.fileSystemAccessor.GetFileName).Take(count).ToList();

            return pathToPackage;
        }

        public IncomingSyncPackage GetSyncItem(string pathToPackage)
        {
            Guid? interviewId = null;
            try
            {
                var policy = SetupRetryPolicyForPackage(pathToPackage);

                Stopwatch innerwatch = Stopwatch.StartNew();
                var fileContent = policy.Execute(() => fileSystemAccessor.ReadAllText(pathToPackage));
                this.logger.Debug($"GetSyncItem {Path.GetFileName(pathToPackage)}: ReadAllText {Path.GetFileName(pathToPackage)}. Took {innerwatch.Elapsed:g}.");
                innerwatch.Stop();

                var syncItem = this.serializer.Deserialize<SyncItem>(fileContent);

                interviewId = syncItem.RootId;

                var meta =
                    this.serializer.Deserialize<InterviewMetaInfo>(archiver.DecompressString(syncItem.MetaInfo));

                var eventsToSync =
                    this.serializer.Deserialize<AggregateRootEvent[]>(archiver.DecompressString(syncItem.Content))
                        .Select(e => e.Payload)
                        .ToArray();

                return new IncomingSyncPackage(meta.PublicKey, meta.ResponsibleId, meta.TemplateId,
                        meta.TemplateVersion, (InterviewStatus)meta.Status, eventsToSync, meta.CreatedOnClient ?? false, syncSettings.Origin, pathToPackage);
            }
            catch (Exception e)
            {
                var message = string.Format("package '{0}' wasn't parsed. Reason: '{1}'", pathToPackage, e.Message);
                this.logger.Error(message, e);
                throw new IncomingSyncPackageException(message, e, interviewId, pathToPackage);
            }
        }
    }
}