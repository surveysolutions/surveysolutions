namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string lastReceivedChunkId);
        string GetLastStoredChunkId();
        string GetChunkBeforeChunkWithId(string before);
    }
}