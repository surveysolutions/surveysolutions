using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Syncronization
{
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message) : base(message)
        {
        }

        protected SynchronizationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SynchronizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}