namespace WB.Core.SharedKernels.Enumerator.Services.Synchronization
{
    public enum SyncStage
    {
        Unknown = 0,
        CheckForObsoleteInterviews,

        CheckObsoleteQuestionnaires,
        DownloadingLogo,
        UploadingAuditLog,
        AssignmentsSynchronization,
        UserAuthentication,
        Success,
        Stopped,
        Canceled,
        Failed,
        FailedAccountIsLockedOnServer,
        FailedUserLinkedToAnotherDevice,
        FailedSupervisorShouldDoOnlineSync,
        FailedUnacceptableSSLCertificate,
        FailedUserDoNotBelongToTeam,
        FailedUpgradeRequired,
        FailedUnexpectedException,
        UploadInterviews,
        CheckNewVersionOfApplication,
        DownloadApplication,
        AttachmentsCleanup,
        UpdatingAssignments,
        UpdatingQuestionnaires,
        FailedUnauthorized
    }
}
