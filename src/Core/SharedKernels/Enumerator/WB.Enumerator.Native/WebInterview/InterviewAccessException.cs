using System;

namespace WB.Enumerator.Native.WebInterview
{
    public class InterviewAccessException : Exception
    {
        public InterviewAccessException(InterviewAccessExceptionReason reason, string message) : base(message)
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
        UserNotAuthorised = 4,
        Forbidden = 5
    }
}