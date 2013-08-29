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

namespace CAPI.Android.Syncronization.RestUtils
{
    public class RestException:Exception
    {
        public RestException()
        {
        }

        public RestException(string message) : base(message)
        {
        }

        protected RestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}