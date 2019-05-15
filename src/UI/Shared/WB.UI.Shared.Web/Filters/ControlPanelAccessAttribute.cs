using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public class ControlPanelAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.IsInRole(nameof(UserRoles.Administrator)) &&
                !filterContext.HttpContext.Request.IsLocal &&
                !CoreSettings.IsDevelopmentEnvironment)
            {
                throw new HttpException(403, "You are not allowed to see contents of this page under current configuration.");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
