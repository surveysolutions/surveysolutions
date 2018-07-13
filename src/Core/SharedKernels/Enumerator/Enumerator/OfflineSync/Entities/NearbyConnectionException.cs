using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Entities
{
    public class NearbyConnectionException : Exception
    {
        public NearbyConnectionException()
        {
        }

        public NearbyConnectionException(string message, ConnectionStatusCode statusCode) : base(message)
        {
        }

        public NearbyConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NearbyConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}