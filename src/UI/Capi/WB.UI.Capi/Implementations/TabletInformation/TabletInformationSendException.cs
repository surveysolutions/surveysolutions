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

namespace WB.UI.Capi.Implementations.TabletInformation
{
    internal class TabletInformationSendException : Exception
    {
        public TabletInformationSendException() {}

        public TabletInformationSendException(string message)
            : base(message) {}

        protected TabletInformationSendException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}

        public TabletInformationSendException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}