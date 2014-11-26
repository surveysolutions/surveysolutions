using System;

namespace WB.UI.Capi.Syncronization
{
    public interface ISyncPackageIdsStorage
    {
        void Append(Guid lastReceivedChunkId);
        Guid? GetLastStoredChunkId();
        Guid? GetChunkBeforeChunkWithId(Guid? before);
    }
}