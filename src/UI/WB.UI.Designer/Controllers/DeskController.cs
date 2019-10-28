using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers
{
    public class DeskController : Controller
    {
        private readonly IDeskAuthenticationService deskAuthenticationService;
        private readonly UserManager<DesignerIdentityUser> users;

        public DeskController(IDeskAuthenticationService deskAuthenticationService,
            UserManager<DesignerIdentityUser> users)
        {
            this.deskAuthenticationService = deskAuthenticationService;
            this.users = users;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null)
                return RedirectToAction("RedirectToDesk");

            return RedirectToPage("/Account/Login", new {area="Identity", returnUrl = Url.Action("RedirectToDesk") });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RedirectToDesk()
        {
            var user = await users.GetUserAsync(User);
            string deskReturnUrl = deskAuthenticationService.GetReturnUrl(
                User.GetId(),
                User.GetUserName(),
                user.Email, 
                DateTime.UtcNow.AddHours(24));
           
            return RedirectPermanent(deskReturnUrl);
        }
    }
}
