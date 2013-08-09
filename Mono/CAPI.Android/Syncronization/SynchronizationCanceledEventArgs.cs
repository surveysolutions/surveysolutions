using System;
using System.Collections.Generic;

namespace CAPI.Android.Syncronization
{
    public class SynchronizationCanceledEventArgs:EventArgs
    {
        public SynchronizationCanceledEventArgs(IList<Exception> exceptions)
        {
            Exceptions = exceptions;
        }

        public IList<Exception> Exceptions { get; private set; }
    }
}