using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Designer.CommonWeb;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Captcha;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly SignInManager<HqUser> signInManager;

        public AccountController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            ICaptchaService captchaService,
            ICaptchaProvider captchaProvider,
            SignInManager<HqUser> signInManager)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult LogOn(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            return this.View(new LogOnModel
            {
                RequireCaptcha = this.captchaService.ShouldShowCaptcha(null)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);

            if (model.RequireCaptcha && !this.captchaProvider.IsCaptchaValid(this))
            {
                this.ModelState.AddModelError("InvalidCaptcha", ErrorMessages.PleaseFillCaptcha);
                return this.View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var signInResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);
            if (signInResult.Succeeded)
            {
                this.captchaService.ResetFailedLogin(model.UserName);
                return Redirect(returnUrl ?? Url.Content("~"));
            }

            if (signInResult.IsLockedOut)
            {
                this.captchaService.ResetFailedLogin(model.UserName);
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }

            this.captchaService.RegisterFailedLogin(model.UserName);
            model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);
            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.IncorrectUserNameOrPassword);
            return View(model);
        }

        public IActionResult LogOff()
        {
            this.signInManager.SignOutAsync();
            return this.Redirect("~/");
        }
    }
}
