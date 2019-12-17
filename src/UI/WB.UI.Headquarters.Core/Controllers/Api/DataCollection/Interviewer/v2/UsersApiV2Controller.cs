using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [Route("api/interviewer/v2/users")]
    public class UsersApiV2Controller : UsersControllerBase
    {
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser,
            SignInManager<HqUser> signInManager,
            IUserViewFactory userViewFactory,
            IApiTokenProvider apiAuthTokenProvider,
            IUserToDeviceService userToDeviceService) : base(
                authorizedUser: authorizedUser,
                userViewFactory: userViewFactory,
                userToDeviceService)
        {
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
        public override ActionResult<InterviewerApiView> Current() => base.Current();

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [Route("hasdevice")]
        public override ActionResult<bool> HasDevice() => base.HasDevice();

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<string>> Login(LogonInfo userLogin)
        {
            var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var signinresult = await this.signInManager.PasswordSignInAsync(userLogin.Username, userLogin.Password, false, false);

            if (signinresult.Succeeded)
            {
                var userId = Guid.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var authToken = await this.apiAuthTokenProvider.GenerateTokenAsync(userId);

                return authToken;
            }

            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }
}
