using System;
using System.Runtime.Serialization;

namespace Synchronization.Core.Errors
{
    public class SynchronizationException : Exception
    {
        public SynchronizationException()
        {
        }

        public SynchronizationException(string message)
            : base(message)
        {
        }

        public SynchronizationException(string mesage, Exception inner)
            : base(mesage, inner)
        {
        }

        /*      public SynchronizationException(string message, Exception innerException) : base(message, innerException)
                {
                }

                protected SynchronizationException(SerializationInfo info, StreamingContext context) : base(info, context)
                {
                }*/
    }

    public class NetUnreachableException : SynchronizationException
    {
        public NetUnreachableException(string url)
            : base(string.Format("Loacl center {0} is not available", url))
        {
        }
    }

    public class UsbUnaccebleException : SynchronizationException
    {
        public UsbUnaccebleException(string message)
            : base(message)
        {
        }
    }

    public class LocalHosUnreachableException : SynchronizationException
    {
        public LocalHosUnreachableException()
            : base("Threre is no connection to local host")
        {
        }
    }
}
