namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IInterviewPackageIdsStorage
    {
        void Store(string packageId, long sortIndex);
        string GetLastStoredPackageId();
    }
}