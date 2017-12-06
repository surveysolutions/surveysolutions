using System.Web.Mvc;


namespace WB.UI.Headquarters.Code
{
    public class AuthorizeOr403Attribute : AuthorizeAttribute
    {
        public AuthorizeOr403Attribute()
        {
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new HttpStatusCodeResult(403);
            else
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }
}