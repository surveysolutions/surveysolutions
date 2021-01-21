
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ActionExecutingContext = Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;
using ActionFilterAttribute = Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute;

namespace WB.UI.Designer.Code
{
    public class LocalOrDevelopmentAccessOnlyAttribute: TypeFilterAttribute
    {
        public LocalOrDevelopmentAccessOnlyAttribute() : base(typeof(LocalOrDevelopmentAccessOnlyFilter))
        {
            Arguments = new object[] { };
        }
    }

    public class LocalOrDevelopmentAccessOnlyFilter: ActionFilterAttribute
    {
        private readonly IHostingEnvironment env;

        public LocalOrDevelopmentAccessOnlyFilter(IHostingEnvironment env)
        {
            this.env = env;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsLocalRequest(filterContext.HttpContext) && !env.IsDevelopment())
            {
                filterContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        public static bool IsLocalRequest(HttpContext context)
        {
            if (context.Connection.RemoteIpAddress == null) return false;
            if (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
            {
                return true;
            }
            if (IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
            {
                return true;
            }
            return false;
        }
    }
}
