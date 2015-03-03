namespace WB.Core.SharedKernels.DataCollection
{
    public interface ISyncProtocolVersionProvider
    {
        int GetProtocolVersion();
        int GetLastNonUpdatableVersion();
    }
}