using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class InterviewException : Exception
    {
        public readonly InterviewDomainExceptionType ExceptionType;

        internal InterviewException(string message, InterviewDomainExceptionType exceptionType = InterviewDomainExceptionType.Undefined)
            : base(message)
        {
            ExceptionType = exceptionType;
        }

        internal InterviewException(string message, Exception innerException,
            InterviewDomainExceptionType exceptionType = InterviewDomainExceptionType.Undefined)
            : base(message, innerException)
        {
            ExceptionType = exceptionType;
        }
    }
}
