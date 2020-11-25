#nullable enable
namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public interface IWorkspaceNameStorage
    {
        void Set(string name);
    }
}
