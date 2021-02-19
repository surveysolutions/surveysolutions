using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleEmail.Model;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Route("api/interviewer/v2/users")]
    public class UsersApiV2Controller : UsersControllerBase
    {
        private readonly UserManager<HqUser> userManager;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser,
            UserManager<HqUser> userManager,
            SignInManager<HqUser> signInManager,
            IUserViewFactory userViewFactory,
            IApiTokenProvider apiAuthTokenProvider,
            IWorkspacesCache workspacesCache,
            IUserToDeviceService userToDeviceService) : base(
                authorizedUser,
                userViewFactory,
                userToDeviceService)
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
            var user = userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id));
            return user.Supervisor.Id;
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("current")]
        [AllowPrimaryWorkspaceFallback]
        [IgnoreWorkspacesLimitation]
        public override ActionResult<InterviewerApiView> Current() => base.Current();

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("hasdevice")]
        public override ActionResult<bool> HasDevice() => base.HasDevice();

        [HttpPost]
        [Route("login")]
        [WriteToSyncLog(SynchronizationLogType.InterviewerLogin)]
        public async Task<ActionResult<string>> Login([FromBody]LogonInfo userLogin)
        {
            var user = await this.userManager.FindByNameAsync(userLogin.Username);

            if (user == null)
                return Unauthorized();

            var signInResult = await this.signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);
            if (signInResult.IsLockedOut)
            {
                return Unauthorized(new {Message = "User is locked"});
            }

            if (signInResult.Succeeded)
            {
                var authToken = await this.apiAuthTokenProvider.GenerateTokenAsync(user.Id);
                return new JsonResult(authToken);
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("changePassword")]
        [WriteToSyncLog(SynchronizationLogType.ChangePassword)]
        public async Task<ActionResult<string>> ChangePassword([FromBody]ChangePasswordInfo userChangePassword)
        {
            var user = await this.userManager.FindByNameAsync(userChangePassword.Username);

            if (user == null)
                return Unauthorized();
            
            var signInResult = await this.signInManager.CheckPasswordSignInAsync(user, userChangePassword.Password, false);
            if (signInResult.IsLockedOut)
            {
                return Unauthorized(new {Message = "User is locked"});
            }

            if (signInResult.Succeeded)
            {
                if (!user.ForceChangePassword)
                    return Forbid();

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, resetToken, userChangePassword.NewPassword);

                if (result.Succeeded)
                {
                    user.ForceChangePassword = false;
                    var updateResult = await userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        var authToken = await this.apiAuthTokenProvider.GenerateTokenAsync(user.Id);
                        return new JsonResult(authToken);
                    }
                    return this.StatusCode(StatusCodes.Status403Forbidden, new ServerError()
                    {
                        Code = ServerErrorCodes.ChangePasswordError,
                        Message = string.Join("\r\n", updateResult.Errors.Select(e => e.Description))
                    });
                }

                return this.StatusCode(StatusCodes.Status403Forbidden, new ServerError()
                {
                    Code = ServerErrorCodes.ChangePasswordError,
                    Message = string.Join("\r\n", result.Errors.Select(e => e.Description))
                });
            }

            return Unauthorized();
        }
    }
}
