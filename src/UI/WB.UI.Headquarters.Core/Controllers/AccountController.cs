using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly SignInManager<HqUser> signInManager;
        private readonly UserManager<HqUser> userManager;
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ILogger<AccountController> logger;

        public AccountController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            ICaptchaService captchaService,
            ICaptchaProvider captchaProvider,
            SignInManager<HqUser> signInManager,
            UserManager<HqUser> userManager,
            IAuthorizedUser authorizedUser,
            ILogger<AccountController> logger)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.authorizedUser = authorizedUser;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult LogOn(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            return this.View(new LogOnModel
            {
                RequireCaptcha = this.captchaService.ShouldShowCaptcha(null)
            });
        }

        [HttpGet]
        public async Task<IActionResult> LogOn2fa(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View(new LogOn2faModel());
        }

        [HttpGet]
        public async Task<IActionResult> LoginWithRecoveryCode()
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View(new LoginWithRecoveryCodeModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;
            
            var isCaptchaRequired = this.captchaService.ShouldShowCaptcha(model.UserName);
            model.RequireCaptcha = isCaptchaRequired;
            
            if (isCaptchaRequired && !await this.captchaProvider.IsCaptchaValid(Request))
            {
                this.ModelState.AddModelError("InvalidCaptcha", ErrorMessages.PleaseFillCaptcha);
                return this.View(model);
            }

            HqUser user = null;

            if (model.UserName != null)
            {
                user = await userManager.FindByNameAsync(model.UserName);
                if (user?.IsInRole(UserRoles.ApiUser) == true)
                {
                    this.ModelState.AddModelError(nameof(model.UserName), ErrorMessages.ApiUserIsNotAllowedToSignIn);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var signInResult = await this.signInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);
            if (signInResult.Succeeded)
            {
                this.captchaService.ResetFailedLogin(model.UserName);

                if (user!.PasswordChangeRequired)
                {
                    var controllerName = nameof(UsersController);
                    var actionName = nameof(UsersController.ChangePassword);
                    return RedirectToAction(actionName, controllerName);
                }

                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (user.IsInRole(UserRoles.Administrator))
                {
                    return Redirect(Url.Content("/administration/Workspaces"));
                }

                return Redirect(Url.Content("~/"));
            }
            if (signInResult.RequiresTwoFactor)
            {
                return RedirectToAction("LogOn2fa", new { ReturnUrl = returnUrl, RememberMe = true });
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogOn2fa(LogOn2faModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl, RememberMe = true });
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var signInResult = await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, false);

            if (signInResult.Succeeded)
            {
                if (user!.PasswordChangeRequired)
                {
                    var controllerName = nameof(UsersController);
                    var actionName = nameof(UsersController.ChangePassword);
                    return RedirectToAction(actionName, controllerName);
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return Redirect(Url.Action("Index", "Home"));
            }

            if (signInResult.IsLockedOut)
            {
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }

            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.InvalidAuthenticatorCode);
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.HasCompanyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey) != null;
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("LogOn", new { ReturnUrl = returnUrl, RememberMe = true });
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var signInResult = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (signInResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return Redirect(Url.Action("Index", "Home"));
            }

            if (signInResult.IsLockedOut)
            {
                this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                return View(model);
            }


            this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.InvalidRecoveryCode);
            return View(model);
        }

        public IActionResult LogOff()
        {
            this.signInManager.SignOutAsync();
            return this.Redirect("~/");
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public async Task<ActionResult> ReturnToObserver()
        {
            if (!this.authorizedUser.IsObserver)
                return NotFound();

            var observerName = User.FindFirst(AuthorizedUser.ObserverClaimType)?.Value;
            var observer = await this.signInManager.UserManager.FindByNameAsync(observerName);

            await this.signInManager.SignOutAsync();
            await this.signInManager.SignInAsync(observer, true);

            return this.Redirect("~/");
        }

        private static readonly Guid[] ObservableRoles = {UserRoles.Headquarter.ToUserId(), UserRoles.Supervisor.ToUserId()};

        [Authorize(Roles = "Administrator, Observer")]
        public async Task<IActionResult> ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName))
                return  NotFound();

            var user = await this.signInManager.UserManager.FindByNameAsync(personName);
            if (user == null || !ObservableRoles.Contains(user.Roles.First().Id))
               return NotFound();

            //do not forget pass current user to display you are observing
            await this.SignInAsObserverAsync(personName);

            return user.IsInRole(UserRoles.Headquarter) ?
                this.RedirectToAction("SurveysAndStatuses", "Reports") :
                this.RedirectToAction("SurveysAndStatusesForSv", "Reports");
        }

        public async Task SignInAsObserverAsync(string userName)
        {
            var observerName = User.FindFirst(AuthorizedUser.ObserverClaimType)?.Value;

            if (observerName != null)
            {
                // do not allow observer to sign in twice.
                // ignoring attempt to sign in
                return;
            }

            var userToObserve = await this.signInManager.UserManager.FindByNameAsync(userName);

            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = AuthorizedUser.ObserverClaimType,
                ClaimValue = authorizedUser.UserName
            });

            userToObserve.Claims.Add(new HqUserClaim
            {
                UserId = userToObserve.Id,
                ClaimType = ClaimTypes.Role,
                ClaimValue = Enum.GetName(typeof(UserRoles), UserRoles.Observer)
            });

            await this.signInManager.SignOutAsync();
            await this.signInManager.SignInAsync(userToObserve, true);
        }
    }
}
