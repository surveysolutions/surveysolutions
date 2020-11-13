using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class PrimaryWorkspaceNameProvider : IWorkspaceNameProvider
    {
        public string CurrentWorkspace()
        {
            return "ws_primary";
        }
    }
}
