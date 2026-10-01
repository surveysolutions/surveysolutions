using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class UsersApiControllerBase: DataCollectionControllerBase
    {
        private readonly HqUserManager userManager;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UsersApiControllerBase(HqUserManager userManager,
            SignInManager<HqUser> signInManager,
            IApiTokenProvider apiAuthTokenProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.apiAuthTokenProvider = apiAuthTokenProvider;
        }
        
        protected async Task<ActionResult<string>> ChangePasswordImplAsync(ChangePasswordInfo userChangePassword)
        {
            var user = await this.userManager.FindByNameAsync(userChangePassword.Username);

            if (user == null)
                return Unauthorized();
            
            var signInResult = await this.signInManager.CheckPasswordSignInAsync(user, userChangePassword.Password, true);
            if (signInResult.IsLockedOut)
            {
                return Unauthorized(new {Message = "User is locked"});
            }

            if (!signInResult.Succeeded) 
                return Unauthorized();
            if (!user.PasswordChangeRequired)
                return Forbid();

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, resetToken, userChangePassword.NewPassword);

            if (!result.Succeeded)
                return this.StatusCode(StatusCodes.Status403Forbidden, new ServerError()
                {
                    Code = ServerErrorCodes.ChangePasswordError,
                    Message = string.Join("\r\n", result.Errors.Select(e => e.Description))
                });
            
            user.PasswordChangeRequired = false;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return this.StatusCode(StatusCodes.Status403Forbidden, new ServerError()
                    {
                        Code = ServerErrorCodes.ChangePasswordError,
                        Message = string.Join("\r\n", updateResult.Errors.Select(e => e.Description))
                    });
            var authToken = await this.apiAuthTokenProvider.GenerateTokenAsync(user.Id);
            return new JsonResult(authToken);
        }
    }
}
