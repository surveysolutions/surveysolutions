using System;
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
        where T : class, IReadSideRepositoryEntity, IIndexedView
    {
        private long currentSortIndex = 1;
        private readonly IReadSideRepositoryWriter writer;
        private readonly IReadSideRepositoryWriter<T> readSideRepositoryWriter;
        private readonly IQueryableReadSideRepositoryReader<T> packageStorageReader;
        public OrderableSyncPackageWriter(
            IReadSideRepositoryWriter<T> readSideRepositoryWriter, 
            IQueryableReadSideRepositoryReader<T> packageStorageReader)
        {
            this.writer = readSideRepositoryWriter as IReadSideRepositoryWriter;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.packageStorageReader = packageStorageReader;
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

        public long GetNextOrder()
        {
            var sortIndex = this.currentSortIndex;
            if (this.writer != null && this.writer.IsCacheEnabled)
            {
                Interlocked.Increment(ref this.currentSortIndex);
                return sortIndex;
            }
            var query = this.packageStorageReader.Query(_ => _.OrderByDescending(x => x.SortIndex).Select(x => x.SortIndex));
            if (query.Any())
            {
                return query.First() + 1;
            }
            return 0;
        }
    }
}