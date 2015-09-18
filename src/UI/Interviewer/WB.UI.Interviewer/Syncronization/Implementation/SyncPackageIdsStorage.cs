using System;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Interviewer.Syncronization.Implementation
{
    internal class SyncPackageIdsStorage : ISyncPackageIdsStorage, IBackupable
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPrincipal principal;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private const string dbFileName = "synchronizationPackages";

        private string FullPathToDataBase
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbFileName);
            }
        }

        public SyncPackageIdsStorage(IFileSystemAccessor fileSystemAccessor, IPrincipal principal)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.principal = principal;
            PluginLoader.Instance.EnsureLoaded();
            this.connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                connection.CreateTable<SyncPackageId>();
            }
        }

        public void Append(string packageId, long sortIndex)
        {
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                var newId = new SyncPackageId
                {
                    PackageId = packageId,
                    SortIndex = sortIndex,
                    UserId = this.principal.CurrentUserIdentity.UserId.FormatGuid(),
                    Type = "Interview"
                };

                connection.Insert(newId);
            }
        }

        public string GetLastStoredPackageId()
        {
            var userIdAsString = this.principal.CurrentUserIdentity.UserId.FormatGuid();
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                var lastStoredChunkId = connection.Table<SyncPackageId>()
                    .Where(x => x.Type == "Interview" && x.UserId == userIdAsString)
                    .OrderBy(x => x.SortIndex)
                    .LastOrDefault();

                if (lastStoredChunkId == null)
                {
                    return null;
                }

                return lastStoredChunkId.PackageId;
            }
        }

        public void CleanAllInterviewIdsForUser(string userId)
        {
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                var interviewRecordsForUser = connection.Table<SyncPackageId>()
                    .Where(x => x.Type == SyncItemType.Interview && x.UserId == userId)
                    .ToList();

                interviewRecordsForUser.ForEach(x => connection.Delete(x));
            }
        }

        public string GetChunkBeforeChunkWithId(string lastKnownPackageId)
        {
            if (string.IsNullOrWhiteSpace(lastKnownPackageId))
            {
                return null;
            }

            var userIdAsString = this.principal.CurrentUserIdentity.UserId.FormatGuid();
            var stringId = lastKnownPackageId;
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                SyncPackageId requestedSortIndex =
                    connection.Table<SyncPackageId>().SingleOrDefault(x => x.PackageId == stringId);

                if (requestedSortIndex == null || requestedSortIndex.SortIndex == 0)
                {
                    return null;
                }

                var prevSortIndex = requestedSortIndex.SortIndex - 1;
                var chunkBeforeChunkWithId =
                    connection.Table<SyncPackageId>()
                        .Where(x => x.UserId == userIdAsString)
                        .SingleOrDefault(x => x.SortIndex == prevSortIndex);
                if (chunkBeforeChunkWithId == null)
                {
                    return null;
                }

                return chunkBeforeChunkWithId.PackageId;
            }
        }

        public string GetPathToBackupFile()
        {
            return this.FullPathToDataBase;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var sourceFile = this.fileSystemAccessor.CombinePath(path, dbFileName);

            this.fileSystemAccessor.DeleteFile(this.FullPathToDataBase);
            this.fileSystemAccessor.CopyFileOrDirectory(sourceFile, Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        }
    }
}