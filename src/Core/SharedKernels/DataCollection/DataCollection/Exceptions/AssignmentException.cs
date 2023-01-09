using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class AssignmentException : Exception
    {
        public AssignmentDomainExceptionType ExceptionType { get; set; }

        public AssignmentException(string message, AssignmentDomainExceptionType? exceptionType = null)
            : base(message)
        {
            this.ExceptionType = exceptionType ?? AssignmentDomainExceptionType.Undefined;
        }

        public AssignmentException(string message, Exception innerException, AssignmentDomainExceptionType? exceptionType = null)
            : base(message, innerException)
        {
            this.ExceptionType = exceptionType ?? AssignmentDomainExceptionType.Undefined;
        }

        protected AssignmentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExceptionType = (AssignmentDomainExceptionType)info.GetInt32(nameof(ExceptionType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ExceptionType), (int)this.ExceptionType);
            base.GetObjectData(info, context);
        }
    }
}
