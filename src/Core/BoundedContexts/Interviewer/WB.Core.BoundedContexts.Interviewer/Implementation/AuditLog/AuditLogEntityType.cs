namespace WB.Core.BoundedContexts.Interviewer.Implementation.AuditLog
{
    public enum AuditLogEntityType
    {
        OpenApplication = 1,
        Login,
        Logout,
        Relink,
        SynchronizationStarted,
        SynchronizationCanceled,
        SynchronizationCompleted,
        CreateInterviewFromAssignment,
        OpenInterview,
        CloseInterview,
        DeleteInterview,
        CompleteInterview
    }
}
