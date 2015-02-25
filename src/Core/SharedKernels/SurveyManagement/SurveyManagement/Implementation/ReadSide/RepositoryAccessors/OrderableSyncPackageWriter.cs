using System;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.RepositoryAccessors
{
    public class OrderableSyncPackageWriter<TMeta, TContent> : IOrderableSyncPackageWriter<TMeta, TContent>
        where TMeta : class, IReadSideRepositoryEntity, IOrderableSyncPackage
        where TContent : class, IReadSideRepositoryEntity, ISyncPackage
    {
        private readonly IChacheableRepositoryWriter writer;
        private readonly IReadSideRepositoryWriter<TMeta> packageMetaWriter;
        private readonly IReadSideKeyValueStorage<TContent> packageContentWriter;

        private static readonly object StoreSyncDeltaLockObject = new object();
        private readonly IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage;

        public OrderableSyncPackageWriter(
            IReadSideRepositoryWriter<TMeta> packageMetaWriter, 
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage, 
            IReadSideKeyValueStorage<TContent> packageContentWriter)
        {
            this.writer = packageMetaWriter as IChacheableRepositoryWriter;
            this.packageMetaWriter = packageMetaWriter;
            this.counterStorage = counterStorage;
            this.packageContentWriter = packageContentWriter;
        }

        public void Store(TContent content, TMeta syncPackageMeta, string partialPackageId, string counterId)
        {
            lock (StoreSyncDeltaLockObject)
            {
                SynchronizationDeltasCounter deltasCounter = this.counterStorage.GetById(counterId);
                int storedDeltasCount = deltasCounter != null ? deltasCounter.CountOfStoredDeltas : 1;

                int nextSortIndex = storedDeltasCount;

                storedDeltasCount++;
                this.counterStorage.Store(new SynchronizationDeltasCounter(storedDeltasCount), counterId);
                
                var packageId = string.Format("{0}${1}", partialPackageId, nextSortIndex);

                syncPackageMeta.SortIndex = nextSortIndex;
                syncPackageMeta.PackageId = packageId;

                content.PackageId = packageId;

                this.packageMetaWriter.Store(syncPackageMeta, packageId);
                this.packageContentWriter.Store(content, packageId);
            }
        }

        public void Clear()
        {
            var repositoryCleaner = this.packageMetaWriter as IReadSideRepositoryCleaner;
            if (repositoryCleaner != null)
            {
                repositoryCleaner.Clear();
            }
        }

        public void EnableCache()
        {
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
                return this.writer != null ? this.writer.ViewType : typeof(TMeta);
            }
        }

        public bool IsCacheEnabled
        {
            get
            {
                return this.writer != null && this.writer.IsCacheEnabled;
            }
        }
    }
}