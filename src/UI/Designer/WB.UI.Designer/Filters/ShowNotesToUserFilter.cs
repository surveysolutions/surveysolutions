using System.Web.Configuration;
using System.Web.Mvc;

namespace WB.UI.Designer.Filters
{
    public class ShowNotesToUserFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller ==null) return;

            var userNotification = WebConfigurationManager.AppSettings["UserNotification"];

            if (!string.IsNullOrWhiteSpace(userNotification))
            {
                controller.ViewData["UserNotification"] = userNotification;
            }
        }
    }
}
