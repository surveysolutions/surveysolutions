using System;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    public class UsersApiV2Controller : UsersControllerBase
    {
        private readonly HqUserManager userManager;

        public UsersApiV2Controller(
            IAuthorizedUser authorizedUser, HqUserManager userManager,
            IUserViewFactory userViewFactory) : base(
                authorizedUser: authorizedUser,
            userViewFactory: userViewFactory)
        {
            
            this.userManager = userManager;
        }

        [HttpGet]
        [ApiBasicAuth(UserRoles.Interviewer)]
        public override InterviewerApiView Current() => base.Current();

        [HttpGet]
        [ApiBasicAuth(UserRoles.Interviewer)]
        public override bool HasDevice() => base.HasDevice();

        [HttpPost]
        public async Task<string> Login(LogonInfo userLogin)
        {
            var hqUser = await this.userManager.FindByNameAsync(userLogin.Username).ConfigureAwait(false);

            if (await this.userManager.CheckPasswordAsync(hqUser, userLogin.Password).ConfigureAwait(false))
            {
                return await this.userManager.GenerateApiAuthTokenAsync(hqUser.Id).ConfigureAwait(false);
            }

            return null;
        }
    }
}