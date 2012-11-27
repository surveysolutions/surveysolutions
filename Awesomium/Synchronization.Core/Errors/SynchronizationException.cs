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

        public SynchronizationException(string message, Exception inner)
            : base(inner == null ? message : message + "\n" + inner.Message, inner)
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
            : base(string.Format("Local center {0} is not available", url))
        {
        }
    }

    public class UsbUnacceptableException : SynchronizationException
    {
        public UsbUnacceptableException(string message)
            : base(message)
        {
        }
    }

    public class UsbNotChoozenException : UsbUnacceptableException
    {
        public UsbNotChoozenException()
            : base("Usb flash memory device has not been choosen")
        {
        }
    }

    public class UsbNotPluggedException : UsbUnacceptableException
    {
        public UsbNotPluggedException()
            : base("Usb flash memory device has not been plugged")
        {
        }
    }

    public class LocalHosUnreachableException : SynchronizationException
    {
        public LocalHosUnreachableException()
            : base("There is no connection to local host")
        {
        }
    }

    public class InactiveNetSynchronizerException : SynchronizationException
    {
        public InactiveNetSynchronizerException()
            : base("Network synchronization endpoint is not set")
        {
        }
    }
}
