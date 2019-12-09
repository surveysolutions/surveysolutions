using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<HqUser> userManager;

        public HomeController(UserManager<HqUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            var roleForCurrentUser = user.Roles.Select(x => x.Id.ToUserRole()).FirstOrDefault();
            
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
