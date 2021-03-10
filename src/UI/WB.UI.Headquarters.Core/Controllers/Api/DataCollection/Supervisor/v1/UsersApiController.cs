using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Route("api/supervisor/v1/users")]
    public class UserControllerBase : ControllerBase
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IUserRepository userViewFactory;
        private readonly SignInManager<HqUser> signInManager;
        private readonly IApiTokenProvider apiAuthTokenProvider;

        public UserControllerBase(
            IAuthorizedUser authorizedUser,
            IUserRepository userViewFactory,
            SignInManager<HqUser> signInManager, 
            IApiTokenProvider apiAuthTokenProvider)
        {
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.signInManager = signInManager;
            this.apiAuthTokenProvider = apiAuthTokenProvider;
            
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        [Route("current")]
        [AllowPrimaryWorkspaceFallback]
        [IgnoreWorkspacesLimitation]
        public virtual SupervisorApiView Current()
        {
            var user = this.userViewFactory.FindById(this.authorizedUser.Id);
            var workspaces = this.authorizedUser.GetEnabledWorkspaces();
            
            return new SupervisorApiView
            {
                Id = user.Id,
                Email = user.Email,
                Workspaces = workspaces.Select(x => new UserWorkspaceApiView
                { 
                    Name = x.Name,
                    DisplayName = x.DisplayName
                }).ToList()
            };
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        [Route("hasdevice")]
        public virtual async Task<bool> HasDevice()
        {
            var user = await this.userViewFactory.FindByIdAsync(this.authorizedUser.Id);
            return !string.IsNullOrEmpty(user.Profile?.DeviceId);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<string>> Login([FromBody]LogonInfo userLogin)
        {
            var user = await this.userViewFactory.FindByNameAsync(userLogin.Username);

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

    }
}
