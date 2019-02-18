using System;

namespace WB.Core.BoundedContexts.Designer.Exceptions
{
    public class ClassificationException : Exception
    {
        public readonly ClassificationExceptionType ErrorType;

        public ClassificationException(string message)
            : this(ClassificationExceptionType.Undefined, message) {}

        public ClassificationException(ClassificationExceptionType errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }
    }

    public enum ClassificationExceptionType
    {
        Undefined = 0,
        NoAccess = 1
    }
}
