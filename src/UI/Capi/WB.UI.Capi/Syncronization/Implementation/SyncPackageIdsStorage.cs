using System;
using System.Linq;
using AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Capi.Syncronization.Implementation
{
    internal class SyncPackageIdsStorage : ISyncPackageIdsStorage
    {
        private readonly SqlitePlainStore plainstore;

        public SyncPackageIdsStorage(SqlitePlainStore plainstore)
        {
            this.plainstore = plainstore;
        }

        public void Append(Guid lastReceivedChunkId)
        {
            var newId = new SyncPackageId();
            newId.Id = lastReceivedChunkId.FormatGuid();
            newId.SortIndex = this.plainstore.Query<SyncPackageId>(x => true).Count();
        }

        public Guid? GetLastStoredChunkId()
        {
            var lastStoredChunkId = this.plainstore.Query<SyncPackageId>(x => true).OrderBy(x => x.SortIndex).Select(x => x.Id).LastOrDefault();
            if (lastStoredChunkId == null)
            {
                return null;
            }

            return Guid.Parse(lastStoredChunkId);
        }

        public Guid? GetChunkBeforeChunkWithId(Guid? before)
        {
            if (!before.HasValue)
            {
                return null;
            }

            var stringId = before.FormatGuid();
            SyncPackageId requestedSortIndex = this.plainstore.Query<SyncPackageId>(x => x.Id == stringId).SingleOrDefault();
            if (requestedSortIndex == null || requestedSortIndex.SortIndex == 0)
            {
                return null;
            }

            int prevSortIndex = requestedSortIndex.SortIndex - 1;
            var chunkBeforeChunkWithId = this.plainstore.Query<SyncPackageId>(x => x.SortIndex == prevSortIndex).Select(x => x.Id).SingleOrDefault();
            if (chunkBeforeChunkWithId == null)
            {
                return null;
            }

            return Guid.Parse(chunkBeforeChunkWithId);
        }
    }
}