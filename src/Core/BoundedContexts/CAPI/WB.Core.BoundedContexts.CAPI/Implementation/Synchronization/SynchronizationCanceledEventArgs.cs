using System;

namespace WB.Core.BoundedContexts.Capi.Implementation.Synchronization
{
    public class SynchronizationCanceledEventArgs:EventArgs
    {
        public SynchronizationCanceledEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}