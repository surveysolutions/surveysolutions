using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Capi.Implementation.Synchronization
{
    public class SynchronizationCanceledEventArgs:EventArgs
    {
        public SynchronizationCanceledEventArgs(IList<Exception> exceptions)
        {
            this.Exceptions = exceptions;
        }

        public IList<Exception> Exceptions { get; private set; }
    }
}