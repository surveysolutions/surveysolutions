using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationAttribute : ActionFilterAttribute
    {
        private IIdentityManager identityManager
        {
            get { return ServiceLocator.Current.GetInstance<IIdentityManager>(); }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is ControlPanelController) return;

            var isInstallController = filterContext.Controller is InstallController;
            var isUserAuthenticated = filterContext.HttpContext.User.Identity.IsAuthenticated;
            var isHQUserExists = identityManager.GetUsersInRole(UserRoles.Headquarter.ToString()).Any();

            if (isInstallController && (isUserAuthenticated || isHQUserExists))
                throw new HttpException(404, string.Empty);

            if (!isInstallController && !isHQUserExists)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(new {controller = "Install", action = "Finish"}));
            }
        }
    }
}