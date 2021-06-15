using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.Migrations.Workspaces
{
    [Migration(202106151719)]
    public class M202106151719_FillWorkspaceIfEmpty: IMigration
    {
        private readonly IPlainStorage<InterviewerIdentity> usersStorage;
        private readonly IPlainStorage<WorkspaceView> workspaces;

        public M202106151719_FillWorkspaceIfEmpty(
            IPlainStorage<InterviewerIdentity> usersStorage,
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

            var interviewerIdentity = usersStorage.LoadAll().SingleOrDefault();
            if (interviewerIdentity == null)
                return;
            
            var workspace = interviewerIdentity.Workspace;
            
            workspaces.Store(new WorkspaceView()
            {
                Id = workspace,
                DisplayName = workspace,
                SupervisorId = interviewerIdentity.SupervisorId,
            });
        }
    }
}