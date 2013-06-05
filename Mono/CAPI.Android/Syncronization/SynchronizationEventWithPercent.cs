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
    public class SynchronizationEventWithPercent:SynchronizationEvent
    {
        public SynchronizationEventWithPercent(string operationTitle, int percent)
            : base(operationTitle)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentException("percent value is incorrect");
            Percent = percent;
        }

        public int Percent { get; private set; }
    }
}