// -----------------------------------------------------------------------
// <copyright file="ServiceError.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Errors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class ServiceException : Exception
    {
        protected ServiceException(string message, Exception e)
            : base(message, e)
        {
        }

        protected ServiceException(string message)
            : base(message)
        {
        }

        public override string Message
        {
            get
            {
                var mess = base.Message;

                var chunks = mess.Split('\n');
                var uniqueChunks =  chunks.Distinct();

                mess = string.Empty;
                foreach (var s in uniqueChunks)
                {
                    var ch = s.Trim('\n');
                    if (string.IsNullOrEmpty(ch))
                        continue;

                    mess += s.Trim('\n') + "\n";
                }


                return mess;
            }
        }
    }

    public class NetIssueException : ServiceException
    {
        public NetIssueException(Exception ex)
            : base("Net access issue", ex)
        {
        }
    }

    public class NetUnreachableException : ServiceException
    {
        public NetUnreachableException(string url)
            : base(string.Format("Local center {0} is not available", url))
        {
        }
    }

    public class UsbNotAccessableException : ServiceException
    {
        public UsbNotAccessableException(string message)
            : base(message)
        {
        }
    }

    public class UsbNotChoozenException : UsbNotAccessableException
    {
        public UsbNotChoozenException()
            : base("Usb flash memory device has not been chosen")
        {
        }
    }

    public class UsbNotPluggedException : UsbNotAccessableException
    {
        public UsbNotPluggedException()
            : base("Usb flash memory device has not been plugged")
        {
        }
    }

    public class LocalHosUnreachableException : ServiceException
    {
        public LocalHosUnreachableException()
            : base("There is no connection to local host")
        {
        }
    }

    public class EndpointNotSetException : ServiceException
    {
        public EndpointNotSetException()
            : base("Network synchronization endpoint is not set")
        {
        }
    }

}
