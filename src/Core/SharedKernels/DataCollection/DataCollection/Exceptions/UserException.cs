using System;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    public class UserException : Exception
    {
        public UserException(string message, UserDomainExceptionType exceptionType = UserDomainExceptionType.Undefined)
            : base(message)
        {
            ExceptionType = exceptionType;
        }

        public UserException(string message, Exception innerException, UserDomainExceptionType exceptionType = UserDomainExceptionType.Undefined)
            : base(message, innerException)
        {
            ExceptionType = exceptionType;
        }

        public readonly UserDomainExceptionType ExceptionType;
    }
}