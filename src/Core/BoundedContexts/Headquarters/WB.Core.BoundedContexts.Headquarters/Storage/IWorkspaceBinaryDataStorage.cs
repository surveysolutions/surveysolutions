using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Storage
{
    public interface IWorkspaceBinaryDataStorage
    {
        Task DeleteAllBinaryDataForWorkspaceAsync();
    }
}
