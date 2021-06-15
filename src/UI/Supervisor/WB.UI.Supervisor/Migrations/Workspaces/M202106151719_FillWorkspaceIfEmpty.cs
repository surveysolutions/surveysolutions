using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Supervisor.Migrations.Workspaces
{
    [Migration(202106151719)]
    public class M202106151719_FillWorkspaceIfEmpty: IMigration
    {
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;
        private readonly IPlainStorage<WorkspaceView> workspaces;

        public M202106151719_FillWorkspaceIfEmpty(
            IPlainStorage<SupervisorIdentity> usersStorage,
            IPlainStorage<WorkspaceView> workspaces)
        {
            this.usersStorage = usersStorage;
            this.workspaces = workspaces;
        }

        public void Up()
        {
            var workspaceViews = workspaces.LoadAll();
            if (workspaceViews.Count > 0)
                return;

            var supervisorIdentity = usersStorage.LoadAll().SingleOrDefault();
            if (supervisorIdentity == null)
                return;
            
            var workspace = supervisorIdentity.Workspace ?? "primary";
            
            workspaces.Store(new WorkspaceView()
            {
                Id = workspace,
                DisplayName = workspace
            });
        }
    }
}