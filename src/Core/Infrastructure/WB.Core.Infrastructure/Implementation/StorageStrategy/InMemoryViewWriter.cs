using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Implementation.StorageStrategy
{
    internal class InMemoryViewWriter<TEntity> : InMemoryViewWriter<TEntity, string>,
        IReadSideStorage<TEntity>, 
        IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        public InMemoryViewWriter(IReadSideStorage<TEntity> readSideRepositoryWriter, Guid viewId)
            : base(readSideRepositoryWriter, viewId.FormatGuid())
        {
        }
    }

    internal class InMemoryViewWriter<TEntity, TKey> : IReadSideStorage<TEntity, TKey>, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private TEntity view;
        private readonly TKey viewId;
        private readonly IReadSideStorage<TEntity, TKey> readSideRepositoryWriter;

        private bool IsDisposed { get; set; }

        public InMemoryViewWriter(IReadSideStorage<TEntity, TKey> readSideRepositoryWriter, TKey viewId)
        {
            this.IsDisposed = false;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.viewId = viewId;
            this.view = readSideRepositoryWriter.GetById(viewId);
        }

        public TEntity GetById(TKey id)
        {
            return this.view;
        }

        public void Store(TEntity projection, TKey id)
        {
            this.view = projection;
        }

        public void BulkStore(List<Tuple<TEntity, TKey>> bulk)
        {
            foreach (var tuple in bulk)
            {
                Store(tuple.Item1, tuple.Item2);
            }
        }

        public void Flush()
        {
            
        }

        public void Remove(TKey id)
        {
            this.view = null;
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

                    this.readSideRepositoryWriter.Flush();
                }
                
                this.IsDisposed = true;
            }
        }
    }
}
