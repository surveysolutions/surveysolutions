﻿using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.Controllers
{
    [ValidateInput(false)]
    [Authorize(Roles = @"Administrator, Headquarter, Supervisor, ApiUser, Observer")]
    public class AccountController : TeamController
    {
        private readonly ICaptchaProvider captchaProvider;
        private readonly ICaptchaService captchaService;
        private readonly HqSignInManager signInManager;
        private readonly IAuthenticationManager authenticationManager;

        public AccountController(
            ICommandService commandService,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            ICaptchaProvider captchaProvider,
            ICaptchaService captchaService,
            HqUserManager userManager,
            HqSignInManager signInManager,
            IAuthenticationManager authenticationManager)
            : base(commandService, logger, authorizedUser, userManager)
        {
            this.captchaProvider = captchaProvider;
            this.captchaService = captchaService;
            this.signInManager = signInManager;
            this.authenticationManager = authenticationManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var roleForCurrentUser = this.authorizedUser.Role;

            switch (roleForCurrentUser)
            {
                case UserRoles.Headquarter:
                    return this.RedirectToAction("SurveysAndStatuses", "HQ");

                case UserRoles.Supervisor:
                    return this.RedirectToAction("Index", "Survey");

                case UserRoles.Administrator:
                case UserRoles.Observer:
                    return this.RedirectToAction("Index", "Headquarters");

                default:
                    return this.RedirectToAction("NotFound", "Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogOn(string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            this.ViewBag.ReturnUrl = returnUrl;

            return this.View(new LogOnModel
            {
                RequireCaptcha = this.captchaService.ShouldShowCaptcha(null)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
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

            var signInResult = await this.signInManager.SignInAsync(model.UserName, model.Password, isPersistent: true);
            switch (signInResult)
            {
                case SignInStatus.Success:
                    this.captchaService.ResetFailedLogin(model.UserName);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    this.captchaService.ResetFailedLogin(model.UserName);
                    this.ModelState.AddModelError("LockedOut", ErrorMessages.SiteAccessNotAllowed);
                    return View(model);
                default:
                    this.captchaService.RegisterFailedLogin(model.UserName);
                    model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);
                    this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.IncorrectUserNameOrPassword);
                    return View(model);
            }
        }

        public ActionResult LogOff()
        {
            this.authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return this.Redirect("~/");
        }

        [Authorize]
        public ActionResult Manage()
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            var currentUser = this.userManager.FindById(this.authorizedUser.Id);

            return View(new ManageAccountModel
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                PersonName = currentUser.FullName,
                PhoneNumber = currentUser.PhoneNumber,
                UserName = currentUser.UserName,
                Role = currentUser.Roles.FirstOrDefault().Role.ToUiString(),
                UpdatePasswordAction = nameof(this.UpdateOwnPassword),
                EditAction = nameof(Manage)
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Manage(ManageAccountModel model)
        {
            var currentUser = this.userManager.FindById(this.authorizedUser.Id);
            model.Id = currentUser.Id;

            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            model.Password = null;
            model.ConfirmPassword = null;
            model.OldPassword = null;

            if (this.ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountAsync(model);

                if (updateResult.Succeeded)
                    this.Success(Strings.HQ_AccountController_AccountUpdatedSuccessfully);
                else
                    this.ModelState.AddModelError("", string.Join(@", ", updateResult.Errors));
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> UpdateOwnPassword(ManageAccountModel model)
        {
            var currentUser = this.userManager.FindById(this.authorizedUser.Id);
            model.Id = currentUser.Id;

            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            await this.ValidateOldPassword(model, currentUser);

            if (this.ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountAsync(model);

                if (updateResult.Succeeded)
                    this.Success(Strings.HQ_AccountController_AccountPasswordChangedSuccessfully);
                else
                    this.ModelState.AddModelError("", string.Join(@", ", updateResult.Errors));
            }
            
            return View("Manage", model);
        }

        private async Task ValidateOldPassword(ManageAccountModel model, HqUser currentUser)
        {
            if (!string.IsNullOrEmpty(model.Password))
            {
                bool isPasswordValid = await this.IsOldPasswordValid(model, currentUser);
                if (!isPasswordValid)
                {
                    this.ModelState.AddModelError<ManageAccountModel>(x => x.OldPassword, FieldsAndValidations.OldPasswordErrorMessage);
                }
            }
        }

        private async Task<bool> IsOldPasswordValid(ManageAccountModel model, HqUser currentUser)
            => !string.IsNullOrEmpty(model.OldPassword)
            && await this.userManager.CheckPasswordAsync(currentUser, model.OldPassword);

        private static readonly UserRoles[] ObservableRoles = {UserRoles.Headquarter, UserRoles.Supervisor};

        [Authorize(Roles = "Administrator, Observer")]
        public async Task<ActionResult> ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName))
                throw new HttpException(404, string.Empty);

            var user = await this.userManager.FindByNameAsync(personName);
            if (user == null)
                throw new HttpException(404, string.Empty);

            if (!ObservableRoles.Contains(user.Roles.First().Role))
                throw new HttpException(404, string.Empty);

            //do not forget pass current user to display you are observing
            await this.signInManager.SignInAsObserverAsync(personName);

            return user.IsInRole(UserRoles.Headquarter) ?
                this.RedirectToAction("SurveysAndStatuses", "HQ") :
                this.RedirectToAction("Index", "Survey");
        }

        private static readonly UserRoles[] CanReturnFromRoles =
        {
            UserRoles.Administrator, UserRoles.Observer, UserRoles.Interviewer
        };

        [Authorize(Roles = "Headquarter, Supervisor")]
        public async Task<ActionResult> ReturnToObserver()
        {
            if (!this.authorizedUser.IsObserver)
                throw new HttpException(404, string.Empty);

            var currentUserRole = this.authorizedUser.Role;

            if (CanReturnFromRoles.Contains(currentUserRole))
                throw new HttpException(404, string.Empty);

            await this.signInManager.SignInBackFromObserverAsync();
            return this.RedirectToAction("Index", "Headquarters");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return RedirectToAction("Index");

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("NotFound", "Error");
        }
        
        [HttpPost]
        public async Task<string> IsUniqueUsername(string userName)
        {
            return await this.userManager.FindByNameAsync(userName) == null
                ? "true"
                : "false";
        }
    }
}