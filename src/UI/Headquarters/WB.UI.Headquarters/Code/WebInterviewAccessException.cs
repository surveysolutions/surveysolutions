using System;

namespace WB.UI.Headquarters.Code
{
    public class WebInterviewAccessException : Exception
    {
        public WebInterviewAccessException(InterviewAccessExceptionReason reason, string message) : base(message)
        {
            this.Reason = reason;
        }

        public InterviewAccessExceptionReason Reason { get; set; }
    }

    public enum InterviewAccessExceptionReason
    {
        InterviewNotFound = 1,
        InterviewExpired = 2,
        NoActionsNeeded = 3,
        UserNotAuthorised = 4
    }
}