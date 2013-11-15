using System;

namespace WB.Core.BoundedContexts.Designer.Exceptions
{
    public class QuestionnaireException : Exception
    {
        public readonly DomainExceptionType ErrorType;

        public QuestionnaireException(string message)
            : this(DomainExceptionType.Undefined, message) {}

        public QuestionnaireException(DomainExceptionType errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }
    }
}