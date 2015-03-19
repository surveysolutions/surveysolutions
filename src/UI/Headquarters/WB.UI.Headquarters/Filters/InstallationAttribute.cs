using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationAttribute : ActionFilterAttribute
    {
        private static bool Installed = false;

        private IIdentityManager identityManager
        {
            get { return ServiceLocator.Current.GetInstance<IIdentityManager>(); }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Installed)
            {

                if (filterContext.Controller is WB.UI.Headquarters.Controllers.ControlPanelController) return;
                if (filterContext.Controller is MaintenanceController) return;

                var isInstallController = filterContext.Controller is InstallController;
            var isAdminExists = identityManager.GetUsersInRole(UserRoles.Administrator.ToString()).Any();

            if (isInstallController && (isAdminExists))
                    throw new HttpException(404, string.Empty);

            if (!isInstallController && !isAdminExists)
                {
                    filterContext.Result =
                        new RedirectToRouteResult(
                            new RouteValueDictionary(new { controller = "Install", action = "Finish" }));
                }

                Installed = true;
            }
        }
    }
}