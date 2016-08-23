using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using Recaptcha;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Membership;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
    [RequireHttps]
    public class AccountController : BaseController
    {
        private readonly ISystemMailer mailer;
        private readonly ILogger logger;
        private readonly IConfigurationManager configurationManager;
        private readonly IAuthenticationService authenticationService;

        public AccountController(IMembershipUserService userHelper, ISystemMailer mailer, ILogger logger,
            IConfigurationManager configurationManager, IAuthenticationService authenticationService) : base(userHelper)
        {
            this.mailer = mailer;
            this.logger = logger;
            this.configurationManager = configurationManager;
            this.authenticationService = authenticationService;
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return this.View();
        }

        public ActionResult Index()
        {
            return this.RedirectToAction("Index", "Questionnaire");
        }

        public ActionResult LogOff()
        {
            this.UserHelper.Logout();

            return this.RedirectToAction("login", "account");
        }

        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Login(string returnUrl)
        {
            return this.View(new LoginModel()
            {
                ShouldShowCaptcha = this.authenticationService.ShouldShowCaptcha(),
                GoogleRecaptchaSiteKey = this.configurationManager.AppSettings["ReCaptchaPublicKey"],
                HomeUrl = Url.Action("Index", "Questionnaire")
            });
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Manage(AccountManageMessageId? message)
        {
            if (message.HasValue)
            {
                this.Success(message.Value.ToUIMessage());
            }

            this.ViewBag.ReturnUrl = this.Url.Action("manage");
            return this.View(new LocalPasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Manage(LocalPasswordModel model)
        {
            this.ViewBag.ReturnUrl = this.Url.Action("manage");
            if (this.ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = this.UserHelper.WebUser.MembershipUser.ChangePassword(
                        model.OldPassword, model.Password);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return this.RedirectToAction("manage", new { message = AccountManageMessageId.ChangePasswordSuccess });
                }
                else
                {
                    this.Error(ErrorMessages.The_current_password_is_incorrect_or_the_new_password_is_invalid);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult PasswordReset()
        {
            if (!Membership.EnablePasswordReset)
            {
                throw new Exception("Password reset is not allowed");
            }

            return this.View(new ResetPasswordModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public async Task<ActionResult> PasswordReset(ResetPasswordModel model)
        {
            if (!Membership.EnablePasswordReset)
            {
                throw new Exception("Password reset is not allowed");
            }

            if (this.ModelState.IsValid)
            {
                MembershipUser user = Membership.GetUser(model.UserName, false);
                if (user == null)
                {
                    this.Error(string.Format(ErrorMessages.User_does_not_exist, model.UserName));
                }
                else
                {
                    string confirmationToken = WebSecurity.GeneratePasswordResetToken(user.UserName);

                    await this.mailer.ResetPasswordEmail(
                        new EmailConfirmationModel()
                            {
                                Email = user.Email.ToWBEmailAddress(),
                                UserName = model.UserName,
                                ConfirmationToken = confirmationToken
                            }).SendAsync();

                    this.Error(ErrorMessages.RestorePassword_EmailSentInstructions);
                    return this.RedirectToAction("Login");
                }
            }

            return this.View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View(new RegisterModel());
        }

        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RecaptchaControlMvc.CaptchaValidatorAttribute]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public async Task<ActionResult> Register(RegisterModel model, bool captchaValid)
        {
            var isUserRegisterSuccessfully = false;
            if (AppSettings.Instance.IsReCaptchaEnabled && !captchaValid)
            {
                this.Error(ErrorMessages.You_did_not_type_the_verification_word_correctly);
            }
            else
            {
                if (this.ModelState.IsValid)
                {
                    // Attempt to register the user
                    try
                    {
                        Guid providerUserKey = Guid.NewGuid();

                        string userName = model.UserName.ToLower();
                        string confirmationToken = WebSecurity.CreateUserAndAccount(
                            userName, model.Password, new { Email = model.Email, ProviderUserKey = providerUserKey }, true);

                        if (!string.IsNullOrEmpty(confirmationToken))
                        {
                            Roles.Provider.AddUsersToRoles(new[] { providerUserKey.ToString() }, new[] { this.UserHelper.USERROLENAME });

                            isUserRegisterSuccessfully = true;

                            await this.mailer.ConfirmationEmail(
                                new EmailConfirmationModel()
                                    {
                                        Email = model.Email.ToWBEmailAddress(),
                                        UserName = userName,
                                        ConfirmationToken = confirmationToken
                                    }).SendAsync();
                        }
                    }
                    catch (MembershipCreateUserException e)
                    {
                        this.Error(e.StatusCode.ToErrorCode());
                    }
                    catch (Exception e)
                    {
                        logger.Error("Register user error", e);
                        this.Error(ErrorMessages.Unexpected_error_occurred_Please_try_again_later);
                    }
                }
            }

            return isUserRegisterSuccessfully ? this.RegisterStepTwo() : this.View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string token)
        {
            if (WebSecurity.ConfirmAccount(token))
            {
                this.Success(ErrorMessages.Your_email_is_verified);
                return this.RedirectToAction("Login");
            }

            return this.RedirectToAction("ConfirmationFailure");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResendConfirmation(string id)
        {
            MembershipUser model = Membership.GetUser(id, false);
            if (model != null)
            {
                if (!model.IsApproved)
                {
                    string token =
                        ((DesignerMembershipProvider)Membership.Provider).GetConfirmationTokenByUserName(model.UserName);
                    if (!string.IsNullOrEmpty(token))
                    {
                        await this.mailer.ConfirmationEmail(
                            new EmailConfirmationModel()
                                {
                                    Email = model.Email.ToWBEmailAddress(),
                                    UserName = model.UserName,
                                    ConfirmationToken = token
                                }).SendAsync();

                        return this.RegisterStepTwo();
                    }
                    else
                    {
                        this.Error(ErrorMessages.Unexpected_error_occurred_Please_try_again_later);
                    }
                }
                else
                {
                    this.Error(string.Format(ErrorMessages.User_already_confirmed_in_system, model.UserName));
                }
            }
            else
            {
                this.Error(string.Format(ErrorMessages.User_does_not_exist_Please_enter_a_valid_user_name, id));
            }

            return this.RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(string token)
        {
            return this.View(new ResetPasswordConfirmationModel { Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult ResetPasswordConfirmation(ResetPasswordConfirmationModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (WebSecurity.ResetPassword(model.Token, model.Password))
                {
                    this.Success(ErrorMessages.Your_password_successfully_changed);
                    return this.RedirectToAction("Login");
                }
                else
                {
                    return this.RedirectToAction("ResetPasswordConfirmationFailure");
                }
            }

            return this.View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmationFailure()
        {
            return this.View();
        }

        private ActionResult RegisterStepTwo()
        {
            return this.View("RegisterStepTwo");
        }

        [HttpPost]
        public JsonResult FindByEmail(string email)
        {
            return Json(new
            {
                doesUserExist = !string.IsNullOrEmpty(Membership.GetUserNameByEmail(email))
            });
        }
    }
}