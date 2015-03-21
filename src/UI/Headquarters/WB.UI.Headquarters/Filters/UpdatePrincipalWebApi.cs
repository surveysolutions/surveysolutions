using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using WB.UI.Headquarters.Code.Security;

namespace WB.UI.Headquarters.Filters
{
    public class UpdatePrincipalWebApi : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            IPrincipal usr = HttpContext.Current.User;
            if (usr.Identity.IsAuthenticated && usr.Identity.AuthenticationType == "Forms")
            {
                FormsIdentity fIdent = usr.Identity as FormsIdentity;
                CustomIdentity ci = new CustomIdentity(fIdent.Ticket);
                CustomPrincipal customPrincipal = new CustomPrincipal(ci);
                HttpContext.Current.User = customPrincipal;
                Thread.CurrentPrincipal = customPrincipal;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}