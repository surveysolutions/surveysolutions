using System;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Capi.Syncronization.Implementation
{
    internal class SyncPackageIdsStorage : ISyncPackageIdsStorage, IBackupable
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISQLiteConnectionFactory connectionFactory;
        private const string dbFileName = "syncPackages";


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

        public void Append(string lastReceivedChunkId)
        {
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            {
                var newId = new SyncPackageId
                {
                    Id = lastReceivedChunkId, 
                    SortIndex = connection.Table<SyncPackageId>().Count()
                };

                connection.Insert(newId);
            }
        }

        public string GetLastStoredChunkId()
        {
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            {
                var lastStoredChunkId = connection.Table<SyncPackageId>().OrderBy(x => x.SortIndex).LastOrDefault();
                if (lastStoredChunkId == null)
                {
                    return null;
                }

                return lastStoredChunkId.Id;
            }
        }

        public string GetChunkBeforeChunkWithId(string before)
        {
            if (string.IsNullOrWhiteSpace(before))
            {
                return null;
            }

            var stringId = before;
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            { 
                SyncPackageId requestedSortIndex = connection.Table<SyncPackageId>().SingleOrDefault(x => x.Id == stringId);
                if (requestedSortIndex == null || requestedSortIndex.SortIndex == 0)
                {
                    return null;
                }

                int prevSortIndex = requestedSortIndex.SortIndex - 1;
                var chunkBeforeChunkWithId = connection.Table<SyncPackageId>().SingleOrDefault(x => x.SortIndex == prevSortIndex);
                if (chunkBeforeChunkWithId == null)
                {
                    return null;
                }

                return chunkBeforeChunkWithId.Id;
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
            this.fileSystemAccessor.CopyFileOrDirectory(sourceFile, FullPathToDataBase);
        }
    }
}