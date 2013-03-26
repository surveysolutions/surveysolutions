namespace WB.UI.Designer.Code.Helpers
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class CommandDeserializationException : Exception
    {
        public CommandDeserializationException() {}

        public CommandDeserializationException(string message)
            : base(message) {}

        public CommandDeserializationException(string message, Exception innerException)
            : base(message, innerException) {}

        protected CommandDeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}