using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1")]
    //[PublicApiJson]
    public class UserController : ControllerBase
    {
        private readonly UserManager<HqUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public UserController(UserManager<HqUser> userManager,
            IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Change own password.
        /// </summary>
        /// <param name="model"></param>
        /// <response code="400">Password cannot be updated.</response>
        /// <response code="200">Password updated.</response>
        [HttpPatch]
        [Route("users/{id}/changepassword")]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Interviewer, UserRoles.Supervisor)]
        public async Task<ActionResult<ChangePasswordResult>> ChangePassword(Guid id, [FromBody, BindRequired]ChangePasswordRequest model)
        {
            if (id != User.UserId())
                return StatusCode(StatusCodes.Status403Forbidden, "You can change only own password");
                
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");
            
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                string passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await this.userManager.ResetPasswordAsync(user, passwordResetToken, model.NewPassword);

                if (resetPasswordResult.Succeeded)
                {
                    if (user.PasswordChangeRequired)
                    {
                        user.PasswordChangeRequired = false;
                        await userManager.UpdateAsync(user);
                    }
                    return new ChangePasswordResult()
                    {
                        Success = true
                    };
                }
                else
                {
                    unitOfWork.DiscardChanges();
                    foreach (var resultError in resetPasswordResult.Errors)
                    {
                        ModelState.AddModelError(resultError.Code, resultError.Description);
                    }
                }
            }
            
            var result = new ChangePasswordResult() { Success = false };

            foreach (var modelState in ModelState.Values) {
                foreach (ModelError error in modelState.Errors) {
                    result.Errors.Add(error.ErrorMessage);
                }
            }

            return BadRequest(result);
        }
    }
}