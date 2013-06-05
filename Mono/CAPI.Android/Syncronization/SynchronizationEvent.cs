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
    public class SynchronizationEvent:EventArgs
    {
        public SynchronizationEvent(string operationTitle)
        {
            OperationTitle = operationTitle;
        }

        public string OperationTitle { get; private set; }
    }
}