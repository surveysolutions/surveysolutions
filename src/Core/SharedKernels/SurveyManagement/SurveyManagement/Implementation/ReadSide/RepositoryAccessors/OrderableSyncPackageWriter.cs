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
        private readonly IReadSideRepositoryWriter<TMeta> packageMetaWriter;
        private readonly IReadSideKeyValueStorage<TContent> packageContentWriter;
        private readonly IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage;

        private static readonly object StoreSyncDeltaLockObject = new object();

        public OrderableSyncPackageWriter(
            IReadSideRepositoryWriter<TMeta> packageMetaWriter, 
            IReadSideKeyValueStorage<SynchronizationDeltasCounter> counterStorage, 
            IReadSideKeyValueStorage<TContent> packageContentWriter)
        {
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
            var packageMetaCacheableWriter = packageMetaWriter as ICacheableRepositoryWriter;
            if (packageMetaCacheableWriter != null)
            {
                packageMetaCacheableWriter.EnableCache();
            }

            var packageContentCacheableWriter = this.packageContentWriter as ICacheableRepositoryWriter;
            if (packageContentCacheableWriter != null)
            {
                packageContentCacheableWriter.EnableCache();
            }

            var counterCacheableWriter = this.counterStorage as ICacheableRepositoryWriter;
            if (counterCacheableWriter != null)
            {
                counterCacheableWriter.EnableCache();
            }
        }

        public void DisableCache()
        {
            var packageMetaCacheableWriter = packageMetaWriter as ICacheableRepositoryWriter;
            if (packageMetaCacheableWriter != null)
            {
                packageMetaCacheableWriter.DisableCache();
            }

            var packageContentCacheableWriter = this.packageContentWriter as ICacheableRepositoryWriter;
            if (packageContentCacheableWriter != null)
            {
                packageContentCacheableWriter.DisableCache();
            }

            var counterCacheableWriter = this.counterStorage as ICacheableRepositoryWriter;
            if (counterCacheableWriter != null)
            {
                counterCacheableWriter.DisableCache();
            }
        }

        public string GetReadableStatus()
        {
            return string.Format("Orderable sync package O_o{0}- {1}{0}- {2}{0}- {3}",
                Environment.NewLine,
                this.packageMetaWriter.GetReadableStatus(),
                this.packageContentWriter.GetReadableStatus(),
                this.counterStorage.GetReadableStatus());
        }

        public Type ViewType {
            get
            {
                return this.packageMetaWriter.ViewType;
            }
        }

        public bool IsCacheEnabled
        {
            get
            {
                var packageMetaCacheableWriter = packageMetaWriter as ICacheableRepositoryWriter;

                return packageMetaCacheableWriter != null && packageMetaCacheableWriter.IsCacheEnabled;
            }
        }
    }
}