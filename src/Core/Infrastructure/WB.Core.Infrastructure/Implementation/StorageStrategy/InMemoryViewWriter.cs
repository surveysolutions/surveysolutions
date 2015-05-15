using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Implementation.StorageStrategy
{
    internal class InMemoryViewWriter<TEntity> : IReadSideStorage<TEntity>, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private TEntity view;
        private readonly Guid viewId;
        private readonly IReadSideStorage<TEntity> readSideRepositoryWriter;

        private bool IsDisposed { get; set; }

        public InMemoryViewWriter(IReadSideStorage<TEntity> readSideRepositoryWriter, Guid viewId)
        {
            this.IsDisposed = false;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.viewId = viewId;
            this.view = readSideRepositoryWriter.GetById(viewId);
        }

        public TEntity GetById(string id)
        {
            return this.view;
        }

        public void Store(TEntity projection, string id)
        {
            this.view = projection;
        }

        public void Remove(string id)
        {
            this.view = null;
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "in-memory-1";
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~InMemoryViewWriter()
        {
             this.Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (this.view != null)
                        this.readSideRepositoryWriter.Store(this.view, this.viewId);
                    else
                    {
                        this.view = this.readSideRepositoryWriter.GetById(this.viewId);
                        if (this.view != null)
                            this.readSideRepositoryWriter.Remove(this.viewId);
                    }
                }

                this.IsDisposed = true;
            }
        }
    }
}
