using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Controllers.Services.Export;
using WB.UI.Shared.Web.Controllers;

namespace WB.UI.Headquarters.Filters
{
    public class InstallationFilter : IActionFilter
    {
        internal static bool Installed = false;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Installed) return;

            switch (filterContext.Controller)
            {
                case ControlPanelController _:
                case UnderConstructionController _:
                case EventsApiController _: // export service connectivity check
                    return;
            }


            var isInstallController = filterContext.Controller is InstallController;
            var adminRole = UserRoles.Administrator.ToUserId();
            IUserRepository userRepository = filterContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            Installed = userRepository.Users.Any(user => user.Roles.Any(role => role.Id == adminRole));

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
