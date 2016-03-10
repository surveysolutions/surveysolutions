using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Main.Core.Events;
using Polly;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
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

            string syncPackageFileName = string.Format("{0}-{1}.{2}", DateTime.Now.Ticks, interviewId.FormatGuid(), this.syncSettings.IncomingCapiPackageFileNameExtension);
            string fullPathToSyncPackage = this.fileSystemAccessor.CombinePath(this.incomingUnprocessedPackagesDirectory, syncPackageFileName);

            this.fileSystemAccessor.WriteAllText(fullPathToSyncPackage, item);
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
                        logger.Warn(
                            string.Format("package '{0}' failed to open with error '{1}'", pathToPackage, exception.Message),
                            exception);
                    }
                );
        }

        public void DeleteSyncItem(string syncItemPath)
        {
            fileSystemAccessor.DeleteFile(syncItemPath);

            var cachedFilesInIncomingDirectory = this.GetCachedFilesInIncomingDirectory();

            cachedFilesInIncomingDirectory.Remove(syncItemPath);

            this.cache.Set("incomingPackagesFileNames", cachedFilesInIncomingDirectory, DateTime.Now.AddMinutes(5));
        }

        private HashSet<string> GetCachedFilesInIncomingDirectory()
        {
            object cachedFileNames = this.cache.Get("incomingPackagesFileNames");

            if (cachedFileNames == null)
            {
                lock (this.cacheLockObject)
                {
                    cachedFileNames = this.cache.Get("incomingPackagesFileNames");

                    if (cachedFileNames == null)
                    {
                        HashSet<string> filenames = this.fileSystemAccessor.GetFilesInDirectory(this.incomingUnprocessedPackagesDirectory).ToHashSet();

                        this.cache.Set("incomingPackagesFileNames", filenames, DateTime.Now.AddMinutes(5));

                        cachedFileNames = filenames;
                    }
                }
            }

            return (HashSet<string>) cachedFileNames;
        }

        public bool HasPackagesByInterviewId(Guid interviewId)
        {
            return
                this.GetCachedFilesInIncomingDirectory().Any(filename
                    => filename.Contains(interviewId.FormatGuid())
                       && filename.EndsWith(this.syncSettings.IncomingCapiPackageFileNameExtension));
        }

        public string DeQueue(int skip)
        {
            var pathToPackage = this.GetCachedFilesInIncomingDirectory().Skip(skip).FirstOrDefault();

            if (string.IsNullOrEmpty(pathToPackage) || !fileSystemAccessor.IsFileExists(pathToPackage))
                return null;

            return pathToPackage;
        }

        public IncomingSyncPackage GetSyncItem(string pathToPackage)
        {
            Guid? interviewId = null;
            try
            {
                var policy = SetupRetryPolicyForPackage(pathToPackage);

                var fileContent = policy.Execute(() => fileSystemAccessor.ReadAllText(pathToPackage));

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
                logger.Error(message, e);
                throw new IncomingSyncPackageException(message, e, interviewId, pathToPackage);
            }
        }
    }
}