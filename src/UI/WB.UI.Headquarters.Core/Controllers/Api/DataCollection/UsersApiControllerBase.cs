using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class UsersApiControllerBase: ControllerBase
    {
        private readonly UserManager<HqUser> userManager;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UsersApiControllerBase(UserManager<HqUser> userManager,
            SignInManager<HqUser> signInManager,
            IApiTokenProvider apiAuthTokenProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.apiAuthTokenProvider = apiAuthTokenProvider;
        }

        public async Task<ActionResult<string>> ChangePassword(ChangePasswordInfo userChangePassword)
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
                if (!user.PasswordChangeRequired)
                    return Forbid();

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, resetToken, userChangePassword.NewPassword);

                if (result.Succeeded)
                {
                    user.PasswordChangeRequired = false;
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