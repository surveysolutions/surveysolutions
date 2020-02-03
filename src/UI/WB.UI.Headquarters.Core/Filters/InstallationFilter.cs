using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationFilter : IActionFilter
    {
        private readonly IUserRepository userRepository;
        internal static bool Installed = false;

        public InstallationFilter(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Installed) return;

            if (filterContext.Controller is ControlPanelController) return;

            if (filterContext.Controller is UnderConstructionController) return;

            var isInstallController = filterContext.Controller is InstallController;
            var adminRole = UserRoles.Administrator.ToUserId();
            Installed = this.userRepository.Users.Any(user => user.Roles.Any(role => role.Id == adminRole));

            if (isInstallController && Installed)
                filterContext.Result = new NotFoundResult();

            if (!isInstallController && !Installed)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Install", action = "Finish" }));
            }
        }
    }
}
