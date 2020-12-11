#nullable enable
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public class WorkspacesUsers
    {
        protected WorkspacesUsers()
        {
            Workspace = null!;
            User = null!;
        }

        public WorkspacesUsers(Workspace workspace, HqUser user)
        {
            Workspace = workspace;
            User = user;
        }

        public virtual int Id { get; protected set; }
        
        public virtual Workspace Workspace { get; protected set; }

        public virtual HqUser User { get; protected set; }
    }
}
