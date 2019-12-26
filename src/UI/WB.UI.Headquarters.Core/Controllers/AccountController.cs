using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Services;
using IdentityResult = Microsoft.AspNetCore.Identity.IdentityResult;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly SignInManager<HqUser> signInManager;
        private readonly HqUserStore userRepository;
        private readonly IAuthorizedUser authorizedUser;

        public AccountController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, 
            ICaptchaService captchaService,
            ICaptchaProvider captchaProvider,
            SignInManager<HqUser> signInManager,
            HqUserStore userRepository,
            IAuthorizedUser authorizedUser)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.signInManager = signInManager;
            this.userRepository = userRepository;
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
                return Redirect(returnUrl ?? Url.Content("~/"));
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

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Manage()
        {
            var manageAccountModel = await GetCurrentUserModelAsync();
            return View(manageAccountModel);
        }

        private async Task<ManageAccountNewModel> GetCurrentUserModelAsync()
        {
            var currentUser = await this.userRepository.FindByIdAsync(this.authorizedUser.Id);
            return new ManageAccountNewModel
            {
                Email = currentUser.Email,
                PersonName = currentUser.FullName,
                PhoneNumber = currentUser.PhoneNumber,
                UserName = currentUser.UserName,
                Role = currentUser.Roles.FirstOrDefault().Id.ToUserRole().ToUiString()
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> UpdateOwnPassword(ManageAccountNewModel model)
        {
            var currentUser = await this.userRepository.FindByIdAsync(this.authorizedUser.Id);
            var resultModel = await GetCurrentUserModelAsync();

            if (!string.IsNullOrEmpty(model.Password))
            {
                bool isPasswordValid = !string.IsNullOrEmpty(model.OldPassword)
                                       && await this.userRepository.CheckPasswordAsync(currentUser, model.OldPassword);
                if (!isPasswordValid)
                {
                    this.ModelState.AddModelError("UpdateOwnPassword." + nameof(ManageAccountNewModel.Password), FieldsAndValidations.OldPasswordErrorMessage);
                }
            }

            if (this.ModelState.IsValid)
            {
                IdentityResult updateResult;

                if (model.Password != null && model.Password == model.ConfirmPassword)
                {
                    if (currentUser == null)
                        updateResult = IdentityResult.Failed(new IdentityError(){Description = FieldsAndValidations.CannotUpdate_CurrentUserDoesNotExists});
                    if (currentUser.IsArchived)
                        updateResult = IdentityResult.Failed(new IdentityError(){Description = FieldsAndValidations.CannotUpdate_CurrentUserIsArchived});

                    updateResult = await this.userRepository.ChangePasswordAsync(currentUser, model.Password);
                }
                else updateResult = IdentityResult.Failed(new IdentityError() {Description = FieldsAndValidations.ConfirmPasswordErrorMassage});

                if (updateResult.Succeeded)
                    this.TempData.Add(Alerts.SUCCESS, Strings.HQ_AccountController_AccountPasswordChangedSuccessfully);
                else
                    this.ModelState.AddModelError("UpdateOwnPassword." + nameof(ManageAccountNewModel.Password), string.Join(@", ", updateResult.Errors));
            }

            return View("Manage", resultModel);
        }

        public IActionResult LogOff()
        {
            this.signInManager.SignOutAsync();
            return this.Redirect("~/");
        }
    }
}
