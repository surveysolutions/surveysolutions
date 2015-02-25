using System;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string packageId, string packageType, Guid userId, long sortIndex);

        string GetChunkBeforeChunkWithId(string lastKnownPackageId, Guid userId);

        string GetLastStoredPackageId(string type, Guid currentUserId);

        void CleanAllInterviewIdsForUser(Guid currentUserId);
    }
}