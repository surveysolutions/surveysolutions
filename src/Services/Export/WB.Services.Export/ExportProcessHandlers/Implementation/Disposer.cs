using System;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class Disposer : IDisposable
    {
        private readonly Action dispose;

        public Disposer(Action dispose)
        {
            this.dispose = dispose;
        }

        private Disposer()
        {
            this.dispose = null;
        }

        public void Dispose()
        {
            dispose?.Invoke();
        }

        public static IDisposable Empty = new Disposer();
    }
}