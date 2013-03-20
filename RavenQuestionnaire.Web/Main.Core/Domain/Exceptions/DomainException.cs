namespace Main.Core.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception which represents domain logic and is usually thrown by aggregate root command handlers.
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        public DomainException() {}

        public DomainException(string message)
            : base(message) {}

        public DomainException(string message, Exception innerException)
            : base(message, innerException) {}

        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}
    }
}