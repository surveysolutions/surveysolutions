using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string packageId, string packageType, Guid userId, int sortIndex);

        string GetChunkBeforeChunkWithId(string type, string lastKnownPackageId, Guid userId);

        string GetLastStoredPackageId(string type, Guid currentUserId);
    }
}