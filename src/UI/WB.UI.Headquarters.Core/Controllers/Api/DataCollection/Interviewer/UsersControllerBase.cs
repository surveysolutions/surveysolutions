using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer
{
    public abstract class UsersControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IUserViewFactory userViewFactory;
        private readonly IUserToDeviceService userToDeviceService;
        private readonly IWorkspacesCache workspacesCache;

        protected UsersControllerBase(
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory,
            IUserToDeviceService userToDeviceService, 
            IWorkspacesCache workspacesCache)
        {
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.userToDeviceService = userToDeviceService;
            this.workspacesCache = workspacesCache;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetInterviewer)]
        public virtual ActionResult<InterviewerApiView> Current()
        {
            var user = this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id));

            var userWorkspaces = this.workspacesCache.CurrentUserWorkspaces();
            var apiViewsForWorkspaces = userWorkspaces.Select(x => new WorkspaceApiView
            {
                Name = x.Name,
                DisplayName = x.DisplayName,
            });

            return new InterviewerApiView
            {
                Id = user.PublicKey,
                SupervisorId = user.Supervisor.Id,
                SecurityStamp = user.SecurityStamp,
                Workspaces = apiViewsForWorkspaces.ToList() 
            };
        }
        
        [WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public virtual ActionResult<bool> HasDevice()=> !string.IsNullOrEmpty(this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id));
    }
}
