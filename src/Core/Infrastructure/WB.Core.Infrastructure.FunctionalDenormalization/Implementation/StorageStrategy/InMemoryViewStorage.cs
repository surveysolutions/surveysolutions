using System;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    public class InMemoryViewStorage<T> : IStorageStrategy<T>, IDisposable where T : class,
        IReadSideRepositoryEntity
    {
        private  T view;
        private readonly Guid id;
        private readonly IReadSideRepositoryWriter<T> readSideRepositoryWriter;

        public bool IsDisposed
        {
            get;
            private set;
        }

        public InMemoryViewStorage(IReadSideRepositoryWriter<T> readSideRepositoryWriter, Guid id)
        {
            this.IsDisposed = false;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.id = id;
            this.view = readSideRepositoryWriter.GetById(id);
        }

        public T Select(Guid id)
        {
            return this.view;
        }

        public void AddOrUpdate(T projection, Guid id)
        {
            this.view = projection;
        }

        public void Delete(T projection, Guid id)
        {
            this.view = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~InMemoryViewStorage()
        {
             Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (view != null)
                        readSideRepositoryWriter.Store(view, id);
                    else
                    {
                        view = readSideRepositoryWriter.GetById(id);
                        if (view != null)
                            readSideRepositoryWriter.Remove(id);
                    }
                }

                IsDisposed = true;
            }
        }
    }
}
