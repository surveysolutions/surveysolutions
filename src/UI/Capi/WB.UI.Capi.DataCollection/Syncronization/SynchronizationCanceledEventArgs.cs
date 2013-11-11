using System;
using System.Collections.Generic;

namespace WB.UI.Capi.DataCollection.Syncronization
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