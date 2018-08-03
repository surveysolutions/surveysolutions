using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity.Owin;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    public class UsersApiV2Controller : UsersControllerBase
    {
        private readonly HqSignInManager signInManager;

        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser, HqSignInManager signInManager,
            IUserViewFactory userViewFactory) : base(
                authorizedUser: authorizedUser,
            userViewFactory: userViewFactory)
        {
            this.signInManager = signInManager;
        }

        [HttpGet]
        [ApiBasicAuth(UserRoles.Interviewer)]
        public Guid Supervisor()
        {
            var user = userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id));
            return user.Supervisor.Id;
        }

        [HttpGet]
        [ApiBasicAuth(UserRoles.Interviewer)]
        public override InterviewerApiView Current() => base.Current();

        [HttpGet]
        [ApiBasicAuth(UserRoles.Interviewer)]
        public override bool HasDevice() => base.HasDevice();

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.InterviewerLogin)]
        public async Task<string> Login(LogonInfo userLogin)
        {
            var signinresult = await this.signInManager.SignInInterviewerAsync(userLogin.Username, userLogin.Password);

            if (signinresult == SignInStatus.Success)
            {
                return await this.signInManager.GenerateApiAuthTokenAsync(authorizedUser.Id);
            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }
    }
}
