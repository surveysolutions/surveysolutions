using System;
using System.Runtime.Serialization;

namespace WB.UI.Shared.Web.CommandDeserialization
{
    [Serializable]
    public class CommandDeserializationException : Exception
    {
        public CommandDeserializationException() {}

        public CommandDeserializationException(string message)
            : base(message) {}

        public CommandDeserializationException(string message, Exception innerException)
            : base(message, innerException) {}        
    }
}
