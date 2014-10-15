using System;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    [DataContract]
    public class InconsistentVersionException : Exception
    {
        public InconsistentVersionException(string message)
            : base(message) {}

        public InconsistentVersionException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
