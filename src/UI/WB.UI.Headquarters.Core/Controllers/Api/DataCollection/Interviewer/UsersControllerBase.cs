using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    public abstract class UsersControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IUserViewFactory userViewFactory;
        private readonly IUserToDeviceService userToDeviceService;

        protected UsersControllerBase(
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory,
            IUserToDeviceService userToDeviceService)
        {
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.userToDeviceService = userToDeviceService;
        }
        
        public virtual ActionResult<InterviewerApiView> Current()
        {
            var user = this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id));

            return new InterviewerApiView
            {
                Id = user.PublicKey,
                SupervisorId = user.Supervisor.Id,
                SecurityStamp = user.SecurityStamp
            };
        }
        
        public virtual ActionResult<bool> HasDevice()=> !string.IsNullOrEmpty(this.userToDeviceService.GetLinkedDeviceId(this.authorizedUser.Id));
    }
}
