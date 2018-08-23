using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationAttribute : ActionFilterAttribute
    {
        private IUserRepository userRepository => ServiceLocator.Current.GetInstance<IUserRepository>();
        internal static bool Installed = false;
        

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Installed) return;

            if (filterContext.Controller is ControlPanelController) return;
            if (filterContext.Controller is MaintenanceController) return;
            if (filterContext.Controller is UnderConstructionController) return;

            var isInstallController = filterContext.Controller is InstallController;
            
            var adminRole = UserRoles.Administrator.ToUserId();
            Installed = this.userRepository.Users.Any(user => user.Roles.Any(role => role.RoleId == adminRole));

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
