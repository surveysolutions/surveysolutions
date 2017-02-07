using System;

namespace WB.UI.Headquarters.Code
{
    public class WebInterviewAccessException : Exception
    {
        public WebInterviewAccessException(ExceptionReason reason, string message) : base(message)
        {
            this.Reason = reason;
        }

        public ExceptionReason Reason { get; set; }

        public enum ExceptionReason
        {
            InterviewNotFound = 1,
            InterviewExpired,
            NoActionsNeeded
        }
    }
}