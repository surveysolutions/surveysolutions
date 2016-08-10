using System.Web.Mvc;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class DeskController : Controller
    {
        private readonly IMembershipUserService userHelper;
        private readonly IConfigurationManager configurationManager;
        private readonly IAuthenticationService authenticationService;
        private readonly IDeskAuthenticationService deskAuthenticationService;

        public DeskController(IMembershipUserService userHelper,
            IConfigurationManager configurationManager, 
            IAuthenticationService authenticationService, 
            IDeskAuthenticationService deskAuthenticationService)
        {
            this.userHelper = userHelper;
            this.configurationManager = configurationManager;
            this.authenticationService = authenticationService;
            this.deskAuthenticationService = deskAuthenticationService;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (this.userHelper.WebUser.MembershipUser != null)
                return RedirectToAction("RedirectToDesk");

            return this.View("~/Views/Account/Login.cshtml", new LoginModel
            {
                ShouldShowCaptcha = this.authenticationService.ShouldShowCaptcha(),
                GoogleRecaptchaSiteKey = this.configurationManager.AppSettings["ReCaptchaPublicKey"],
                HomeUrl = Url.Action("RedirectToDesk")
            });
        }

        [HttpGet]
        [Authorize]
        public ActionResult RedirectToDesk()
        {
            string deskReturnUrl = deskAuthenticationService.GetReturnUrl(
                userHelper.WebUser.UserId,
                this.userHelper.WebUser.UserName,
                this.userHelper.WebUser.MembershipUser.Email);
           
            return RedirectPermanent(deskReturnUrl);
        }
    }
}
