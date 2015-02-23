using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors
{
    public class OrderableSyncPackageWriter<T> : IOrderableSyncPackageWriter<T>
        where T : class, IReadSideRepositoryEntity, ISyncPackage
    {
        private long currentSortIndex = 1;
        private readonly IChacheableRepositoryWriter writer;
        private readonly IReadSideRepositoryWriter<T> readSideRepositoryWriter;
        private readonly IQueryableReadSideRepositoryReader<T> packageStorageReader;

        private static readonly object StoreSyncDeltaLockObject = new object();
        private readonly IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage;

        public OrderableSyncPackageWriter(
            IReadSideRepositoryWriter<T> readSideRepositoryWriter, 
            IQueryableReadSideRepositoryReader<T> packageStorageReader, 
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage)
        {
            this.writer = readSideRepositoryWriter as IChacheableRepositoryWriter;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.packageStorageReader = packageStorageReader;
            this.counterStorage = counterStorage;
        }

        public void Clear()
        {
            var repositoryCleaner = readSideRepositoryWriter as IReadSideRepositoryCleaner;
            if (repositoryCleaner != null)
            {
                repositoryCleaner.Clear();
            }
        }

        public void EnableCache()
        {
            this.currentSortIndex = 1;
            if (this.writer != null)
            {
                this.writer.EnableCache();
            }
        }

        public void DisableCache()
        {
            if (this.writer != null)
            {
                this.writer.DisableCache();
            }
        }

        public string GetReadableStatus()
        {
            return this.writer != null ? this.writer.GetReadableStatus() : string.Empty;
        }

        public Type ViewType {
            get
            {
                return this.writer != null ? this.writer.ViewType : typeof(T);
            }
        }

        public bool IsCacheEnabled
        {
            get
            {
                return this.writer != null && this.writer.IsCacheEnabled;
            }
        }

        public T GetById(string id)
        {
            return this.readSideRepositoryWriter.GetById(id);
        }

        public void Remove(string id)
        {
            this.readSideRepositoryWriter.Remove(id);
        }

        public void Store(T view, string id)
        {
            this.readSideRepositoryWriter.Store(view, id);
        }

        public void StoreNextPackage(string counterId, Func<int, T> createSyncPackage)
        {
            lock (StoreSyncDeltaLockObject)
            {
                SynchronizationDeltasCounter deltasCounter = this.counterStorage.GetById(counterId);
                int storedDeltasCount = deltasCounter != null ? deltasCounter.CountOfStoredDeltas : 1;

                int nextSortIndex = storedDeltasCount;

                storedDeltasCount++;
                this.counterStorage.Store(new SynchronizationDeltasCounter(storedDeltasCount), counterId);

                T synchronizationDelta = createSyncPackage(nextSortIndex);

                this.readSideRepositoryWriter.Store(synchronizationDelta, synchronizationDelta.PackageId);
            }
        }

        public void BulkStore(List<Tuple<T, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }
    }
}