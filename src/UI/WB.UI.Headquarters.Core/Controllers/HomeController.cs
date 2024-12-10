using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HqUserManager userManager;
        private readonly SignInManager<HqUser> signInManager;
        
        public HomeController(HqUserManager userManager, 
            SignInManager<HqUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await this.signInManager.SignOutAsync();
                return Redirect("Index");
            }
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
                    return this.RedirectToAction("Headquarters", "Users");

                case UserRoles.Interviewer:
                    return this.RedirectToAction("CreateNew", "InterviewerHq");

                default:
                    return NotFound();
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
