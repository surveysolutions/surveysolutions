namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public enum SyncDeclineReason
    {
        Unknown = 0,
        UnexpectedClientVersion = 1,
        NotATeamMember = 2,
        InvalidLoginToken = 3,
        UserIsLocked = 4
    }
}
