using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.UI.Headquarters.API.Filters;

namespace WB.UI.Headquarters.Code
{
    public class CustomWebApiAuthorizeFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            bool markedWithAllowAnonymousAttribute = 
                actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0
                || 
                actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0;


            bool markedServiceApiKeyAuthorizationAttribute = 
                actionContext.ActionDescriptor.GetCustomAttributes<ServiceApiKeyAuthorizationAttribute>().Count > 0
                || 
                actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<ServiceApiKeyAuthorizationAttribute>().Count > 0;
            
            if (markedWithAllowAnonymousAttribute || markedServiceApiKeyAuthorizationAttribute)
            {
                return;
            }

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }
    }
}
