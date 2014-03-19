using System.Web.Http.Controllers;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Api.Authentication
{
    public class BasicAuthenticationAttribute : BasicAuthenticationFilter
    {
        [Inject]
        public ISupervisorLoginService SupervisorLoginService { get; set; }

        protected override bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            return this.SupervisorLoginService.AreCredentialsValid(username, password);
        }
    }
}