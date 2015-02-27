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
        private readonly IChacheableRepositoryWriter packageMetaCacheableWriter;
        private readonly IReadSideRepositoryWriter<TMeta> packageMetaWriter;
        private readonly IReadSideKeyValueStorage<TContent> packageContentWriter;

        private static readonly object StoreSyncDeltaLockObject = new object();
        private readonly IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage;

        public OrderableSyncPackageWriter(
            IReadSideRepositoryWriter<TMeta> packageMetaWriter, 
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage, 
            IReadSideKeyValueStorage<TContent> packageContentWriter)
        {
            this.packageMetaCacheableWriter = packageMetaWriter as IChacheableRepositoryWriter;
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
            var packageMetaCleaner = this.packageMetaWriter as IReadSideRepositoryCleaner;
            if (packageMetaCleaner != null)
            {
                packageMetaCleaner.Clear();
            }

            var packageContenCleaner = this.packageContentWriter as IReadSideRepositoryCleaner;
            if (packageContenCleaner != null)
            {
                packageContenCleaner.Clear();
            }

            var counterStorageCleaner = this.counterStorage as IReadSideRepositoryCleaner;
            if (counterStorageCleaner != null)
            {
                counterStorageCleaner.Clear();
            }
        }

        public void EnableCache()
        {
            if (this.packageMetaCacheableWriter != null)
            {
                this.packageMetaCacheableWriter.EnableCache();
            }

            var packageContentCacheableWriter = this.packageContentWriter as IChacheableRepositoryWriter;
            if (packageContentCacheableWriter != null)
            {
                packageContentCacheableWriter.EnableCache();
            }

            var counterCacheableWriter = this.counterStorage as IChacheableRepositoryWriter;
            if (counterCacheableWriter != null)
            {
                counterCacheableWriter.EnableCache();
            }
        }

        public void DisableCache()
        {
            if (this.packageMetaCacheableWriter != null)
            {
                this.packageMetaCacheableWriter.DisableCache();
            }

            var packageContentCacheableWriter = this.packageContentWriter as IChacheableRepositoryWriter;
            if (packageContentCacheableWriter != null)
            {
                packageContentCacheableWriter.DisableCache();
            }

            var counterCacheableWriter = this.counterStorage as IChacheableRepositoryWriter;
            if (counterCacheableWriter != null)
            {
                counterCacheableWriter.DisableCache();
            }
        }

        public string GetReadableStatus()
        {
            return this.packageMetaCacheableWriter != null ? this.packageMetaCacheableWriter.GetReadableStatus() : string.Empty;
        }

        public Type ViewType {
            get
            {
                return this.packageMetaCacheableWriter != null ? this.packageMetaCacheableWriter.ViewType : typeof(TMeta);
            }
        }

        public bool IsCacheEnabled
        {
            get
            {
                return this.packageMetaCacheableWriter != null && this.packageMetaCacheableWriter.IsCacheEnabled;
            }
        }
    }
}