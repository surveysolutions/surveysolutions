using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class InterviewException : Exception
    {
        public readonly InterviewDomainExceptionType ExceptionType;

        public InterviewException(string message, InterviewDomainExceptionType? exceptionType = null)
            : base(message)
        {
            this.ExceptionType = exceptionType ?? InterviewDomainExceptionType.Undefined;
        }

        public InterviewException(string message, Exception innerException, InterviewDomainExceptionType? exceptionType = null)
            : base(message, innerException)
        {
            this.ExceptionType = exceptionType ?? InterviewDomainExceptionType.Undefined;
        }
    }
}
