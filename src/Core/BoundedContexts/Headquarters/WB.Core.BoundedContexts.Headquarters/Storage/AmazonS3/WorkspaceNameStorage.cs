#nullable enable
using System;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class WorkspaceNameStorage : IWorkspaceNameProvider, IWorkspaceNameStorage
    {
        private string? currentWorkspace;

        public string CurrentWorkspace()
        {
            return currentWorkspace ?? WorkspaceConstants.DefaultWorkspacename;
        }

        public void Set(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (currentWorkspace != null)
            {
                throw new InvalidOperationException("Workspace has already being set")
                {
                    Data =
                    {
                        { "currentWorkspace", currentWorkspace },
                        { "incomingWorkspace", name }
                    }
                };
            }
            
            this.currentWorkspace = name;
        }
    }
}
