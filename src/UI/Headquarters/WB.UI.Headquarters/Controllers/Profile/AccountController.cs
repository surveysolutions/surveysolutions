using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.Owin.Security;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Captcha;

namespace WB.UI.Headquarters.Controllers
{
    [ValidateInput(false)]
    [AuthorizeOr403(Roles = @"Administrator, Headquarter, Supervisor, ApiUser, Observer, Interviewer")] // UserRoles.
    public class AccountController : TeamController
    {
        private readonly ICaptchaProvider captchaProvider;
        private readonly ICaptchaService captchaService;
        private readonly HqSignInManager signInManager;
        private readonly IAuthenticationManager authenticationManager;
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;

        public AccountController(
            ICommandService commandService,
            ILogger logger,
            IAuthorizedUser authorizedUser,
            ICaptchaProvider captchaProvider,
            ICaptchaService captchaService,
            HqUserManager userManager,
            HqSignInManager signInManager,
            IAuthenticationManager authenticationManager, IPlainKeyValueStorage<CompanyLogo> appSettingsStorage, IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage)
            : base(commandService, logger, authorizedUser, userManager, profileSettingsStorage)
        {
            this.captchaProvider = captchaProvider;
            this.captchaService = captchaService;
            this.signInManager = signInManager;
            this.authenticationManager = authenticationManager;
            this.appSettingsStorage = appSettingsStorage;
        }


        [Authorize]
        public async Task<ActionResult> Manage()
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            var manageAccountModel = await GetCurrentUserModel();
            manageAccountModel.AllowEditLockState = false;
            return View(manageAccountModel);
        }

        private async Task<ManageAccountModel> GetCurrentUserModel()
        {
            var currentUser = await this.userManager.FindByIdAsync(this.authorizedUser.Id);
            var manageAccountModel = new ManageAccountModel
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                PersonName = currentUser.FullName,
                PhoneNumber = currentUser.PhoneNumber,
                UserName = currentUser.UserName,
                Role = currentUser.Roles.FirstOrDefault().Id.ToUserRole().ToUiString(),
                UpdatePasswordAction = nameof(this.UpdateOwnPassword),
                EditAction = nameof(Manage)
            };
            return manageAccountModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Manage(ManageAccountModel model)
        {
            var currentUser = await this.userManager.FindByIdAsync(this.authorizedUser.Id);
            model.Id = currentUser.Id;

            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            model.AllowEditLockState = false;
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

            model.EditAction = nameof(Manage);
            model.UpdatePasswordAction = nameof(this.UpdateOwnPassword);
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> UpdateOwnPassword([Bind(Prefix = "UpdateOwnPassword")]ManageAccountModel model)
        {
            var currentUser = await this.userManager.FindByIdAsync(this.authorizedUser.Id);
            var resultModel = await GetCurrentUserModel();

            model.Id = currentUser.Id;

            this.ViewBag.ActivePage = MenuItem.ManageAccount;
            model.AllowEditLockState = false;

            if (!string.IsNullOrEmpty(model.Password))
            {
                bool isPasswordValid = await this.IsOldPasswordValid(model, currentUser);
                if (!isPasswordValid)
                {
                    this.ModelState.AddModelError("UpdateOwnPassword." + nameof(ManageAccountModel.Password), FieldsAndValidations.OldPasswordErrorMessage);
                }
            }

            if (this.ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountPasswordAsync(model);

                if (updateResult.Succeeded)
                    this.Success(Strings.HQ_AccountController_AccountPasswordChangedSuccessfully);
                else
                    this.ModelState.AddModelError("UpdateOwnPassword." + nameof(ManageAccountModel.Password), string.Join(@", ", updateResult.Errors));
            }

            model.EditAction = nameof(Manage);
            model.UpdatePasswordAction = nameof(this.UpdateOwnPassword);
            return View("Manage", resultModel);
        }

        private async Task<bool> IsOldPasswordValid(ManageAccountModel model, HqUser currentUser)
            => !string.IsNullOrEmpty(model.OldPassword)
            && await this.userManager.CheckPasswordAsync(currentUser, model.OldPassword);

        private static readonly Guid[] ObservableRoles = {UserRoles.Headquarter.ToUserId(), UserRoles.Supervisor.ToUserId()};

        [AuthorizeOr403(Roles = "Administrator, Observer")]
        public async Task<ActionResult> ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName))
                throw new HttpException(404, string.Empty);

            var user = await this.userManager.FindByNameAsync(personName);
            if (user == null)
                throw new HttpException(404, string.Empty);

            if (!ObservableRoles.Contains(user.Roles.First().Id))
                throw new HttpException(404, string.Empty);

            //do not forget pass current user to display you are observing
            await this.signInManager.SignInAsObserverAsync(personName);

            return user.IsInRole(UserRoles.Headquarter) ?
                this.RedirectToAction("SurveysAndStatuses", "Reports") :
                this.RedirectToAction("SurveysAndStatusesForSv", "Reports");
        }

        private static readonly UserRoles[] CanReturnFromRoles =
        {
            UserRoles.Administrator, UserRoles.Observer, UserRoles.Interviewer
        };

        [AuthorizeOr403(Roles = "Headquarter, Supervisor")]
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
