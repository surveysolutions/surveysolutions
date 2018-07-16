namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog
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
        SynchronizationFailed,
        CreateInterviewFromAssignment,
        OpenInterview,
        CloseInterview,
        DeleteInterview,
        CompleteInterview,
        ApproveInterview,
        RejectInterview,
    }
}
