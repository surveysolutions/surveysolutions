using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class InterviewException : Exception
    {
        public readonly InterviewDomainExceptionType ErrorType;

        internal InterviewException(string message, InterviewDomainExceptionType errorType = InterviewDomainExceptionType.Undefined)
            : base(message)
        {
            ErrorType = errorType;
        }

        internal InterviewException(string message, Exception innerException,
            InterviewDomainExceptionType errorType = InterviewDomainExceptionType.Undefined)
            : base(message, innerException)
        {
            ErrorType = errorType;
        }
    }
}
