using System.Web.Configuration;
using System.Web.Mvc;
using WB.UI.Designer.Controllers;

namespace WB.UI.Designer.Filters
{
    public class ShowNotesToUserFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as AccountController;
            if (controller ==null) return;

            var userNotification = WebConfigurationManager.AppSettings["UserNotification"];

            if (!string.IsNullOrWhiteSpace(userNotification))
            {
                controller.ViewData["UserNotification"] = userNotification;
            }
        }
    }
}