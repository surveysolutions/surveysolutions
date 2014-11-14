using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    internal class InMemoryViewWriter<T> : IReadSideRepositoryWriter<T>, IDisposable where T : class,
        IReadSideRepositoryEntity
    {
        private  T view;
        private readonly Guid viewId;
        private readonly IReadSideRepositoryWriter<T> readSideRepositoryWriter;

        public bool IsDisposed
        {
            get;
            private set;
        }

        public InMemoryViewWriter(IReadSideRepositoryWriter<T> readSideRepositoryWriter, Guid viewId)
        {
            this.IsDisposed = false;
            this.readSideRepositoryWriter = readSideRepositoryWriter;
            this.viewId = viewId;
            this.view = readSideRepositoryWriter.GetById(viewId);
        }

        public T GetById(string id)
        {
            return this.view;
        }

        public void Store(T projection, string id)
        {
            this.view = projection;
        }

        public void Remove(string id)
        {
            this.view = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~InMemoryViewWriter()
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
                        readSideRepositoryWriter.Store(view, this.viewId);
                    else
                    {
                        view = readSideRepositoryWriter.GetById(this.viewId);
                        if (view != null)
                            readSideRepositoryWriter.Remove(this.viewId);
                    }
                }

                IsDisposed = true;
            }
        }
    }
}
