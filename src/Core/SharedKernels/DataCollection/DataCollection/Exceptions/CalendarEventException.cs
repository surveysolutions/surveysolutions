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
    }
}
