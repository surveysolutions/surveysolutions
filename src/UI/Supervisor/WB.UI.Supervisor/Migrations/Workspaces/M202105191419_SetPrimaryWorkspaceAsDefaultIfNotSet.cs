using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Supervisor.Migrations.Workspaces
{
    [Migration(202105191419)]
    public class M202105191419_SetPrimaryWorkspaceAsDefaultIfNotSet: IMigration
    {
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;

        public M202105191419_SetPrimaryWorkspaceAsDefaultIfNotSet(
            IPlainStorage<SupervisorIdentity> usersStorage)
        {
            this.usersStorage = usersStorage;
        }

        public void Up()
        {
            var supervisorIdentity = usersStorage.LoadAll().SingleOrDefault();
            if (supervisorIdentity == null)
                return;
            
            var workspace = supervisorIdentity.Workspace;
            if (!string.IsNullOrWhiteSpace(workspace))
                return;

            supervisorIdentity.Workspace = "primary";
            usersStorage.Store(supervisorIdentity);
        }
    }
}