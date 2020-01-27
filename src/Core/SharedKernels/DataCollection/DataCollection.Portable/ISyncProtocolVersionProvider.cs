namespace WB.Core.SharedKernels.DataCollection
{
    public interface ISyncProtocolVersionProvider
    {
        int GetProtocolVersion();
        int GetLastNonUpdatableVersion();

        int[] GetBlackListedBuildNumbers();
    }

    public interface IInterviewerSyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
    }

    public interface ISupervisorSyncProtocolVersionProvider : ISyncProtocolVersionProvider
    {
    }
}
