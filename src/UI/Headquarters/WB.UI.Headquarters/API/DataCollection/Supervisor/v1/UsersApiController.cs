using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity.Owin;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    public class UserApiController : ApiController
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IUserViewFactory userViewFactory;
        protected readonly HqSignInManager signInManager;

        public UserApiController(
            IAuthorizedUser authorizedUser,
            IUserViewFactory userViewFactory,
            HqSignInManager signInManager)
        {
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [ApiBasicAuth(UserRoles.Supervisor)]
        //[WriteToSyncLog(SynchronizationLogType.GetSupervisor)]
        public virtual SupervisorApiView Current()
        {
            var user = this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id));

            return new SupervisorApiView()
            {
                Id = user.PublicKey,
            };
        }

        [HttpGet]
        [ApiBasicAuth(UserRoles.Supervisor)]
        //[WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public virtual bool HasDevice() => !string.IsNullOrEmpty(this.authorizedUser.DeviceId);

        [HttpPost]
        //[WriteToSyncLog(SynchronizationLogType.SupervisorLogin)]
        public async Task<string> Login(LogonInfo userLogin)
        {
            var signInResult = await this.signInManager.SignInSupervisorAsync(userLogin.Username, userLogin.Password);

            if (signInResult == SignInStatus.Success)
            {
                return await this.signInManager.GenerateApiAuthTokenAsync(authorizedUser.Id);
            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }

    }
}
