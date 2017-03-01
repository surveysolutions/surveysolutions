using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Identity;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationAttribute : ActionFilterAttribute
    {
        private IIdentityManager identityManager => ServiceLocator.Current.GetInstance<IIdentityManager>();
        internal static bool Installed = false;
        

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Installed) return;

            if (filterContext.Controller is ControlPanelController) return;
            if (filterContext.Controller is MaintenanceController) return;

            var isInstallController = filterContext.Controller is InstallController;

            Installed = identityManager.HasAdministrator;

            if (isInstallController && Installed)
                throw new HttpException(404, string.Empty);

            if (!isInstallController && !Installed)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(new {controller = "Install", action = "Finish"}));
            }
        }
    }
}