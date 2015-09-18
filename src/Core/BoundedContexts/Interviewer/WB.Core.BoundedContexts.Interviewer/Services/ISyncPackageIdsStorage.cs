namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface ISyncPackageIdsStorage
    {
        void Append(string packageId, long sortIndex);
        string GetChunkBeforeChunkWithId(string lastKnownPackageId);
        string GetLastStoredPackageId();
        void CleanAllInterviewIdsForUser(string userId);
    }
}