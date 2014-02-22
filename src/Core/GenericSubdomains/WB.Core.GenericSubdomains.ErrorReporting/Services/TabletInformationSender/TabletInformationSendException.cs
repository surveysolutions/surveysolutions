using System;
using System.Runtime.Serialization;

namespace WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender
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