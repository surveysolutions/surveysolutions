using System.Diagnostics;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;

        public HomeController(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        public IActionResult Index()
        {
            var roleForCurrentUser = this.authorizedUser.Role;

            switch (roleForCurrentUser)
            {
                case UserRoles.Headquarter:
                    return this.RedirectToAction("SurveysAndStatuses", "Reports");

                case UserRoles.Supervisor:
                    return this.RedirectToAction("SurveysAndStatusesForSv", "Reports");

                case UserRoles.Administrator:
                    return this.RedirectToAction("SurveysAndStatuses", "Reports");

                case UserRoles.Observer:
                    return this.RedirectToAction("Index", "Headquarters");

                case UserRoles.Interviewer:
                    return this.RedirectToAction("CreateNew", "InterviewerHq");

                default:
                    return this.RedirectToAction("NotFound", "Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
