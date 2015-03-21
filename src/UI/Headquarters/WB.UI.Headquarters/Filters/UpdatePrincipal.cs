using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WB.UI.Headquarters.Code.Security;

namespace WB.UI.Headquarters.Filters
{
    public class UpdatePrincipal : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
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