namespace WB.Core.SharedKernels.DataCollection.Views.AuditLog
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
