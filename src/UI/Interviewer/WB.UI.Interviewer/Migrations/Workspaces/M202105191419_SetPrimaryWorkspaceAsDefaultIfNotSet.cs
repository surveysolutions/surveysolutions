using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Migrations.Workspaces
{
    [Migration(202105191419)]
    public class M202105191419_SetPrimaryWorkspaceAsDefaultIfNotSet: IMigration
    {
        private readonly IPlainStorage<InterviewerIdentity> usersStorage;

        public M202105191419_SetPrimaryWorkspaceAsDefaultIfNotSet(
            IPlainStorage<InterviewerIdentity> usersStorage)
        {
            this.usersStorage = usersStorage;
        }

        public void Up()
        {
            var interviewerIdentity = usersStorage.LoadAll().SingleOrDefault();
            if (interviewerIdentity == null)
                return;
            
            var workspace = interviewerIdentity.Workspace;
            if (!string.IsNullOrWhiteSpace(workspace))
                return;

            interviewerIdentity.Workspace = "primary";
            usersStorage.Store(interviewerIdentity);
        }
    }
}