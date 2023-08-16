using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class InterviewException : Exception
    {
        public InterviewDomainExceptionType ExceptionType { get; set; }

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

        protected InterviewException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExceptionType = (InterviewDomainExceptionType)info.GetInt32(nameof(ExceptionType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ExceptionType), (int)this.ExceptionType);
            base.GetObjectData(info, context);
        }
    }
}
