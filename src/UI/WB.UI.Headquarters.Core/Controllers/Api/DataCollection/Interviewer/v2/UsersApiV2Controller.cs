using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Route("api/interviewer/v2/users")]
    public class UsersApiV2Controller : UsersControllerBase
    {
        private readonly HqUserManager userManager;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser,
            HqUserManager userManager,
            SignInManager<HqUser> signInManager,
            IUserRepository userViewFactory,
            IApiTokenProvider apiAuthTokenProvider,
            IUserToDeviceService userToDeviceService) : base(
                authorizedUser,
                userViewFactory,
                userToDeviceService,
                userManager, signInManager, apiAuthTokenProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.apiAuthTokenProvider = apiAuthTokenProvider;
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("supervisor")]
        public Guid Supervisor()
        {
            var user = userViewFactory.FindById(this.authorizedUser.Id);
            return user.WorkspaceProfile.SupervisorId 
                   ?? throw new ArgumentException("SupervisorId must be set for interviewer");
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("current")]
        [AllowPrimaryWorkspaceFallback]
        [IgnoreWorkspacesLimitation]
        [AllowDisabledWorkspaceAccess]
        public override ActionResult<InterviewerApiView> Current() => base.Current();

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("hasdevice")]
        public override ActionResult<bool> HasDevice() => base.HasDevice();

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [WriteToSyncLog(SynchronizationLogType.InterviewerLogin)]
        public async Task<ActionResult<string>> Login([FromBody]LogonInfo userLogin)
        {
            var user = await this.userManager.FindByNameAsync(userLogin.Username);

            if (user == null || String.IsNullOrEmpty(userLogin.Password))
                return Unauthorized();

            var signInResult = await this.signInManager.CheckPasswordSignInAsync(user, userLogin.Password, true);
            if (signInResult.IsLockedOut)
            {
                return Unauthorized(new {Message = "User is locked"});
            }

            if (!signInResult.Succeeded) 
                return Unauthorized();
            var authToken = await this.apiAuthTokenProvider.GenerateTokenAsync(user.Id);
            return new JsonResult(authToken);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("changePassword")]
        [WriteToSyncLog(SynchronizationLogType.ChangePassword)]
        public Task<ActionResult<string>> ChangePassword([FromBody] ChangePasswordInfo userChangePassword)
            => base.ChangePasswordImplAsync(userChangePassword);
    }
}
