﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly SignInManager<HqUser> signInManager;
        protected readonly IAuthorizedUser authorizedUser;
        
        public AccountController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            ICaptchaService captchaService,
            ICaptchaProvider captchaProvider,
            SignInManager<HqUser> signInManager,
            IAuthorizedUser authorizedUser)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.signInManager = signInManager;
            this.authorizedUser = authorizedUser;
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

            if (model.RequireCaptcha && !await this.captchaProvider.IsCaptchaValid(Request))
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
                return Redirect(returnUrl ?? Url.Action("Index", "Home"));
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

        [Authorize(Roles = "Headquarter, Supervisor")]
        public async Task<ActionResult> ReturnToObserver()
        {
            if (!this.authorizedUser.IsObserver)
                NotFound();

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
                NotFound();

            var user = await this.signInManager.UserManager.FindByNameAsync(personName);
            if (user == null)
                NotFound();

            if (!ObservableRoles.Contains(user.Roles.First().Id))
                NotFound();

            //do not forget pass current user to display you are observing
            await this.SignInAsObserverAsync(personName);

            return user.IsInRole(UserRoles.Headquarter) ?
                this.RedirectToAction("SurveysAndStatuses", "Reports") :
                this.RedirectToAction("SurveysAndStatusesForSv", "Reports");
        }

        public async Task SignInAsObserverAsync(string userName)
        {
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
