using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WB.Core.SharedKernels.DataCollection.Exceptions
{
    [Serializable]
    public class CalendarEventException : Exception
    {
        public CalendarEventDomainExceptionType ExceptionType { get; set; }

        public CalendarEventException(string message, CalendarEventDomainExceptionType? exceptionType = null)
            : base(message)
        {
            this.ExceptionType = exceptionType ?? CalendarEventDomainExceptionType.Undefined;
        }

        public CalendarEventException(string message, Exception innerException, CalendarEventDomainExceptionType? exceptionType = null)
            : base(message, innerException)
        {
            this.ExceptionType = exceptionType ?? CalendarEventDomainExceptionType.Undefined;
        }

        protected CalendarEventException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExceptionType = (CalendarEventDomainExceptionType)info.GetInt32(nameof(ExceptionType));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ExceptionType), (int)this.ExceptionType);
            base.GetObjectData(info, context);
        }
    }
}