using System;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.UI.Capi.Syncronization.Implementation
{
    internal class SyncPackageIdsStorage : ISyncPackageIdsStorage, IBackupable
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private const string oldDbFileName = "syncPackages";
        private const string dbFileName = "synchronizationPackages";

        private string FullPathToDataBase
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbFileName);
            }
        }

        public SyncPackageIdsStorage(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            PluginLoader.Instance.EnsureLoaded();
            connectionFactory = Mvx.GetSingleton<ISQLiteConnectionFactory>();
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            {
                connection.CreateTable<SyncPackageId>();
            }
        }

        public void Append(string packageId, string packageType, Guid userId, long sortIndex)
        {
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            {
                var newId = new SyncPackageId
                {
                    PackageId = packageId,
                    SortIndex = sortIndex,
                    UserId = userId.FormatGuid(),
                    Type = packageType
                };

                connection.Insert(newId);
            }
        }

        public string GetLastStoredPackageId(string type, Guid currentUserId)
        {
            return this.LastStoredPackageId(type, currentUserId);
        }

        public void CleanAllInterviewIdsForUser(Guid userId)
        {
            var userIdAsString = userId.FormatGuid();
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                var interviewRecordsForUser = connection.Table<SyncPackageId>()
                    .Where(x => x.Type == SyncItemType.Interview && x.UserId == userIdAsString)
                    .ToList();

                interviewRecordsForUser.ForEach(x => connection.Delete(x));
            }
        }

        private string LastStoredPackageId(string type, Guid userId)
        {
            var userIdAsString = userId.FormatGuid();
            using (var connection = this.connectionFactory.Create(this.FullPathToDataBase))
            {
                var lastStoredChunkId = connection.Table<SyncPackageId>()
                    .Where(x => x.Type == type && x.UserId == userIdAsString)
                    .OrderBy(x => x.SortIndex)
                    .LastOrDefault();

                if (lastStoredChunkId == null)
                {
                    return null;
                }

                return lastStoredChunkId.PackageId;
            }
        }

        public string GetChunkBeforeChunkWithId(string lastKnownPackageId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(lastKnownPackageId))
            {
                return null;
            }

            var userIdAsString = userId.FormatGuid();
            var stringId = lastKnownPackageId;
            using (var connection = connectionFactory.Create(FullPathToDataBase))
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

            this.fileSystemAccessor.DeleteFile(FullPathToDataBase);
            this.fileSystemAccessor.CopyFileOrDirectory(sourceFile, Environment.GetFolderPath(Environment.SpecialFolder.Personal));
        }
    }
}