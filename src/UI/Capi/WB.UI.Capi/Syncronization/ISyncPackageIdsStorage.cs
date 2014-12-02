using System;

namespace WB.UI.Capi.Syncronization
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string lastReceivedChunkId);
        Guid? GetLastStoredChunkId();
        Guid? GetChunkBeforeChunkWithId(Guid? before);
    }
}