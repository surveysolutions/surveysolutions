using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public ActionResult Login()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                
            }

            return this.View(model);
        }
    }
}