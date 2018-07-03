namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public enum SynchronizationExceptionType
    {
        Unexpected,
        RequestByTimeout,
        RequestCanceledByUser,
        HostUnreachable,
        InvalidUrl,
        NoNetwork,
        UserLocked,
        UserNotApproved,
        UserIsNotInterviewer,
        Unauthorized,
        Maintenance,
        UpgradeRequired,
        ServiceUnavailable,
        InternalServerError,
        UserLinkedToAnotherDevice,
        NotSupportedServerSyncProtocolVersion,
        UnacceptableSSLCertificate
    }
}