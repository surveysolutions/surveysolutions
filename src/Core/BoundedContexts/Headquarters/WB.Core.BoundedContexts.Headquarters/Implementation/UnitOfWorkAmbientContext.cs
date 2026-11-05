using System;
using System.Threading;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public static class UnitOfWorkAmbientContext
    {
        private static readonly AsyncLocal<IUnitOfWork> current = new();

        public static IUnitOfWork Current => current.Value;

        public static IDisposable Push(IUnitOfWork unitOfWork)
        {
            var previous = current.Value;
            current.Value = unitOfWork;
            return new RestoreScope(previous);
        }

        private sealed class RestoreScope : IDisposable
        {
            private readonly IUnitOfWork previous;
            private bool disposed;

            public RestoreScope(IUnitOfWork previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                current.Value = previous;
                disposed = true;
            }
        }
    }
}
