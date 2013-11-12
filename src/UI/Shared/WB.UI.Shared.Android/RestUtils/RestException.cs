using System;
using System.Runtime.Serialization;

namespace WB.UI.Shared.Android.RestUtils
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