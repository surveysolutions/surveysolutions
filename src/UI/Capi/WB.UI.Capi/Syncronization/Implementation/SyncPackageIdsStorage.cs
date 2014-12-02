using System;
using System.IO;
using System.Linq;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Capi.Syncronization.Implementation
{
    internal class SyncPackageIdsStorage : ISyncPackageIdsStorage
    {
        private readonly ISQLiteConnectionFactory connectionFactory;

        private string FullPathToDataBase
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "syncPackages");
            }
        }

        public SyncPackageIdsStorage()
        {
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

        public Guid? GetLastStoredChunkId()
        {
            using (var connection = connectionFactory.Create(FullPathToDataBase))
            {
                var lastStoredChunkId = connection.Table<SyncPackageId>().OrderBy(x => x.SortIndex).LastOrDefault();
                if (lastStoredChunkId == null)
                {
                    return null;
                }

                return Guid.Parse(lastStoredChunkId.Id);
            }
        }

        public Guid? GetChunkBeforeChunkWithId(Guid? before)
        {
            if (!before.HasValue)
            {
                return null;
            }

            var stringId = before.FormatGuid();
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

                return Guid.Parse(chunkBeforeChunkWithId.Id);
            }
        }
    }
}