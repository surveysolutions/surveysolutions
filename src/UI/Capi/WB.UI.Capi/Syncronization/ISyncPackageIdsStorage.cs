namespace WB.UI.Capi.Syncronization
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string lastReceivedChunkId);
        string GetLastStoredChunkId();
        string GetChunkBeforeChunkWithId(string before);
    }
}