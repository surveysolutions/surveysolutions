using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;

namespace WB.UI.Designer1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICaptchaProtectedAuthenticationService captchaProtectedAuthentication;

        public AccountController(ICaptchaProtectedAuthenticationService captchaProtectedAuthentication)
        {
            this.captchaProtectedAuthentication = captchaProtectedAuthentication;
        }

        [AllowAnonymous]
        [ResponseCache(NoStore = true)]
        public ActionResult Login(string returnUrl)
        {
            return this.View(new LogonModel
            {
                ShouldShowCaptcha = this.captchaProtectedAuthentication.ShouldShowCaptcha(),
                StaySignedIn = false
            });
        }

        public IActionResult Register()
        {
            throw new System.NotImplementedException();
        }

        public IActionResult PasswordReset()
        {
            throw new System.NotImplementedException();
        }
    }
}
