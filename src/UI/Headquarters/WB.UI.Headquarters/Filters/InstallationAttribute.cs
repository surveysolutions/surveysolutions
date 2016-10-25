using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationAttribute : ActionFilterAttribute
    {
        internal static bool Installed = false;

        private IIdentityManager identityManager => ServiceLocator.Current.GetInstance<IIdentityManager>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Installed) return;

            if (filterContext.Controller is Controllers.ControlPanelController) return;
            if (filterContext.Controller is MaintenanceController) return;

            var isInstallController = filterContext.Controller is InstallController;
            Installed = identityManager.GetUsersInRole(UserRoles.Administrator.ToString()).Any();

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