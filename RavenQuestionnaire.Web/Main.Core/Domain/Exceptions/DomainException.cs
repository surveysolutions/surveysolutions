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
        public readonly DomainExceptionType ErrorType;

        public DomainException(DomainExceptionType errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }
    }
}