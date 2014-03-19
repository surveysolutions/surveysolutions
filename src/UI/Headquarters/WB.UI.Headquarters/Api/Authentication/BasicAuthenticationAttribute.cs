using System.Web.Http.Controllers;
using Microsoft.AspNet.Identity;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.UI.Headquarters.Api.Authentication
{
    public class BasicAuthenticationAttribute : BasicAuthenticationFilter
    {
        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        protected override bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            ApplicationUser applicationUser = UserManager.Find(username, password);

            return applicationUser != null;
        }
    }
}