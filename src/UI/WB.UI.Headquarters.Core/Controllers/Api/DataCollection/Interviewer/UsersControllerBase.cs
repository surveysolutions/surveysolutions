using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer
{
    public abstract class UsersControllerBase : UsersApiControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IUserRepository userViewFactory;
        private readonly IUserToDeviceService userToDeviceService;

        protected UsersControllerBase(
            IAuthorizedUser authorizedUser,
            IUserRepository userViewFactory,
            IUserToDeviceService userToDeviceService,
            UserManager<HqUser> userManager,
            SignInManager<HqUser> signInManager,
            IApiTokenProvider apiAuthTokenProvider)
            :base(userManager, signInManager, apiAuthTokenProvider)
        {
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.userToDeviceService = userToDeviceService;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetInterviewer)]
        public virtual ActionResult<InterviewerApiView> Current()
        {
            var user = this.userViewFactory.FindById(this.authorizedUser.Id);

            var userWorkspaces = user.Workspaces
                .Where(w => w.Workspace.DisabledAtUtc == null);
            
            var apiViewsForWorkspaces = userWorkspaces.Select(x => new UserWorkspaceApiView
            {
                Name = x.Workspace.Name,
                DisplayName = x.Workspace.DisplayName,
                SupervisorId = x.SupervisorId,
            });

            return new InterviewerApiView
            {
                Id = user.Id,
                SupervisorId = user.Profile.SupervisorId!.Value,
                SecurityStamp = user.SecurityStamp,
                Workspaces = apiViewsForWorkspaces.ToList() 
            };
        }
        
        [WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public virtual ActionResult<bool> HasDevice()=> !string.IsNullOrEmpty(this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id));
    }
}
