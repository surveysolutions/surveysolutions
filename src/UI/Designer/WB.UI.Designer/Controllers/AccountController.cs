using System;
using System.Configuration.Provider;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Captcha;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
    [RequireHttps]
    public class AccountController : BaseController
    {
        private readonly ISystemMailer mailer;
        private readonly ILogger logger;
        private readonly IAuthenticationService authenticationService;
        private readonly ICaptchaService captchaService;
        private readonly ICaptchaProvider captchaProvider;
        private readonly IAccountRepository accountRepository;

        public AccountController(IMembershipUserService userHelper, ISystemMailer mailer, ILogger logger,
            IAuthenticationService authenticationService, 
            ICaptchaService captchaService, ICaptchaProvider captchaProvider, IAccountRepository accountRepository) : base(userHelper)
        {
            this.mailer = mailer;
            this.logger = logger;
            this.authenticationService = authenticationService;
            this.captchaService = captchaService;
            this.captchaProvider = captchaProvider;
            this.accountRepository = accountRepository;
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
            return this.View(new LogonModel
            {
                ShouldShowCaptcha = this.authenticationService.ShouldShowCaptcha(),
                StaySignedIn = false
            });
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Manage(AccountManageMessageId? message)
        {
            if (message.HasValue)
            {
                this.Success(message.Value.ToUIMessage());
            }

            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            return this.View(new ManageAccountModel
            {
                ChangePassword = new ChangePasswordModel(),
                UserProfile = new UpdateUserProfileModel
                {
                    FullName = this.UserHelper.WebUser.MembershipUser.FullName,
                    Email = this.UserHelper.WebUser.MembershipUser.Email
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Manage(UpdateUserProfileModel model)
        {
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
            if (this.ModelState.IsValid)
            {
                try
                {
                    var user = (DesignerMembershipUser) Membership.GetUser(this.UserHelper.WebUser.UserId, false);

                    Membership.UpdateUser(
                        new DesignerMembershipUser(
                            providerName: user.ProviderName,
                            name: user.UserName,
                            providerUserKey: user.ProviderUserKey,
                            email: model.Email,
                            passwordQuestion: user.PasswordQuestion,
                            comment: user.Comment,
                            isApproved: user.IsApproved,
                            isLockedOut: user.IsLockedOut,
                            creationDate: user.CreationDate,
                            lastLoginDate: user.LastLoginDate,
                            lastActivityDate: user.LastActivityDate,
                            lastPasswordChangedDate: user.LastPasswordChangedDate,
                            lastLockoutDate: user.LastLockoutDate,
                            canImportOnHq: user.CanImportOnHq,
                            fullName: model.FullName));

                    return this.RedirectToAction("Manage", new {message = AccountManageMessageId.UpdateUserProfileSuccess});
                }
                catch (ProviderException e)
                {
                    this.Error(e.Message);
                }
                catch (Exception e)
                {
                    this.logger.Error("User update exception", e);
                    this.Error(ErrorMessages.UnhandledExceptionDuringUpdateUserInfo);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View(new ManageAccountModel
            {
                ChangePassword = new ChangePasswordModel(),
                UserProfile = model
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            this.ViewBag.ReturnUrl = this.Url.Action("Manage");
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
                    return this.RedirectToAction("Manage",
                        new {message = AccountManageMessageId.ChangePasswordSuccess});
                }
                else
                {
                    this.Error(ErrorMessages.The_current_password_is_incorrect_or_the_new_password_is_invalid);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View("Manage", new ManageAccountModel
            {
                ChangePassword = model,
                UserProfile = new UpdateUserProfileModel
                {
                    FullName = this.UserHelper.WebUser.MembershipUser.FullName,
                    Email = this.UserHelper.WebUser.MembershipUser.Email
                }
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult PasswordReset()
        {
            if (!Membership.EnablePasswordReset)
            {
                throw new Exception(ErrorMessages.PasswordResetNotAllowed);
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
                throw new Exception(ErrorMessages.PasswordResetNotAllowed);
            }

            if (this.ModelState.IsValid)
            {
                var user = this.accountRepository.GetByNameOrEmail(model.UserName);
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
                                UserName = user.FullName ?? user.UserName,
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
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            var isUserRegisterSuccessfully = false;
            if (AppSettings.Instance.IsReCaptchaEnabled && !this.captchaProvider.IsCaptchaValid(this))
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
                            userName, model.Password, new { Email = model.Email, FullName = model.FullName, ProviderUserKey = providerUserKey }, true);

                        if (!string.IsNullOrEmpty(confirmationToken))
                        {
                            Roles.Provider.AddUsersToRoles(new[] { providerUserKey.ToString() }, new[] { this.UserHelper.USERROLENAME });

                            isUserRegisterSuccessfully = true;

                            await this.mailer.ConfirmationEmail(
                                new EmailConfirmationModel()
                                    {
                                        Email = model.Email.ToWBEmailAddress(),
                                        UserName = model.FullName ?? userName,
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

        private bool ShouldShowCaptchaByUserName(string userName)
        {
            return this.authenticationService.ShouldShowCaptcha() ||
                   this.authenticationService.ShouldShowCaptchaByUserName(userName);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Login(LogonModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                this.captchaService.RegisterFailedLogin(model.UserName);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
            }

            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
             {
                this.captchaService.RegisterFailedLogin(model.UserName);
                this.Error(ErrorMessages.EmptyUserNameOrPassword);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
             }

             var shouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);

             if (shouldShowCaptcha && !this.captchaProvider.IsCaptchaValid(this))
             {
                this.Error(ErrorMessages.You_did_not_type_the_verification_word_correctly);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
             }

             var user = this.accountRepository.GetByNameOrEmail(model.UserName);
             if (user == null)
             {
                this.captchaService.RegisterFailedLogin(model.UserName);

                this.Error(ErrorMessages.IncorrectUserNameOrPassword);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
             }

             if (user.IsLockedOut)
             {
                this.captchaService.RegisterFailedLogin(model.UserName);
                this.Error(ErrorMessages.AccountBlocked);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
             }

            if (!user.IsConfirmed)
            {
                var message = string.Format(ErrorMessages.ConfirmAccount, user.Email) + 
                    $" <a href='{GlobalHelper.GenerateUrl("ResendConfirmation", "Account", new { id = user.UserName })}'>{ErrorMessages.RequestAnotherEmail}</a>";

                this.Error(message);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
                
            }

            var userIsAuthorized = this.authenticationService.Login(model.UserName, model.Password, model.StaySignedIn);

            if (!userIsAuthorized && !shouldShowCaptcha && this.ShouldShowCaptchaByUserName(model.UserName))
            {
                //Captcha Required
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
            }

            if (!userIsAuthorized)
            {
                this.captchaService.RegisterFailedLogin(model.UserName);
                this.Error(ErrorMessages.IncorrectUserNameOrPassword);
                model.ShouldShowCaptcha = this.ShouldShowCaptchaByUserName(model.UserName);
                return this.View(model);
            }

            return this.RedirectToLocal(returnUrl: returnUrl);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToAction("Index", "Questionnaire");
            
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);

            return RedirectToAction("NotFound", "Error");
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
        public JsonResult FindByEmail(string emailOrLogin)
        {
            var account = accountRepository.GetByNameOrEmail(emailOrLogin);
            return Json(new
            {
                doesUserExist = !string.IsNullOrEmpty(account?.UserName),
                userName = account?.UserName,
                email = account?.Email,
                id = account?.ProviderUserKey
            });
        }
    }
}
