using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.AspNet.Identity.Owin;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
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
        private readonly HqUserManager userManager;

        public AccountController(
            ICommandService commandService, 
            ILogger logger,
            IIdentityManager identityManager,
            ICaptchaProvider captchaProvider, 
            ICaptchaService captchaService,
            HqUserManager userManager)
            : base(commandService, logger, identityManager)
        {
            this.captchaProvider = captchaProvider;
            this.captchaService = captchaService;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var roleForCurrentUser = await this.identityManager.GetRoleForCurrentUserAsync();

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

            if (!ModelState.IsValid)            {
                return View(model);
            }
            
            var signInResult = await this.identityManager.SignInAsync(model.UserName, model.Password, isPersistent: true);
            switch (signInResult)
            {
                case SignInStatus.Success:
                    this.captchaService.ResetFailedLogin(model.UserName);
                    return RedirectToLocal(returnUrl);
                default:
                    this.captchaService.RegisterFailedLogin(model.UserName);
                    model.RequireCaptcha = this.captchaService.ShouldShowCaptcha(model.UserName);
                    this.ModelState.AddModelError("InvalidCredentials", ErrorMessages.IncorrectUserNameOrPassword);
                    return View(model);
            }
        }

        public ActionResult LogOff()
        {
            this.identityManager.SignOut();
            return this.Redirect("~/");
        }

        [Authorize]
        public ActionResult Manage()
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            var currentUser = this.identityManager.CurrentUser;

            return View(new ManageAccountModel()
            {
                Id = currentUser.Id,
                Email = currentUser.Email,
                PersonName = currentUser.FullName,
                PhoneNumber = currentUser.PhoneNumber
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Manage(ManageAccountModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            if (!string.IsNullOrEmpty(model.Password))
            {
                bool isPasswordValid = await this.userManager.CheckPasswordAsync(this.identityManager.CurrentUser, model.OldPassword);
                if (!isPasswordValid)
                {
                    this.ModelState.AddModelError<ManageAccountModel>(x=> x.OldPassword, FieldsAndValidations.OldPasswordErrorMessage);
                }
            }

            if (this.ModelState.IsValid)
            {
                await this.UpdateAccountAsync(model);
                this.Success(Strings.HQ_AccountController_AccountUpdatedSuccessfully);
            }
            
            return this.View(model);
        }


        [Authorize(Roles = "Administrator, Observer")]
        public async Task<ActionResult> ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName))
                throw new HttpException(404, string.Empty);

            var user = await this.identityManager.GetUserByNameAsync(personName);
            if (user == null)
                throw new HttpException(404, string.Empty);

            if (!new[] {UserRoles.Headquarter, UserRoles.Supervisor}.Contains(user.Roles.First().Role))
                throw new HttpException(404, string.Empty);

            //do not forget pass current user to display you are observing
            await this.identityManager.SignInAsObserverAsync(personName);

            return this.identityManager.IsCurrentUserHeadquarter ?
                this.RedirectToAction("SurveysAndStatuses", "HQ") :
                this.RedirectToAction("Index", "Survey");
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public async Task<ActionResult> ReturnToObserver()
        {
            if (!this.identityManager.IsCurrentUserObserver)
                throw new HttpException(404, string.Empty);

            var currentUserRole = await this.identityManager.GetRoleForCurrentUserAsync();

            if (new[] { UserRoles.Administrator, UserRoles.Observer, UserRoles.Interviewer }.Contains(currentUserRole))
                throw new HttpException(404, string.Empty);

            await this.identityManager.SignInBackFromObserverAsync();
            return this.RedirectToAction("Index", "Headquarters");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return RedirectToAction("Index");

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("NotFound", "Error");
        }
    }
}