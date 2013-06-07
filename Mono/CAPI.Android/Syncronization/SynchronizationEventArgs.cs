using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Syncronization
{
    public class SynchronizationEventArgs:EventArgs
    {
        public SynchronizationEventArgs(string operationTitle, Operation operationType, bool cancelable)
        {
            OperationTitle = operationTitle;
            OperationType = operationType;
            Cancelable = cancelable;
        }

        public string OperationTitle { get; private set; }
        public Operation OperationType { get; private set; }
        public bool Cancelable { get; private set; }
    }

    public enum Operation
    {
        Handshake,
        Pull,
        Push,
        Validation
    }
}