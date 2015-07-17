using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using Recaptcha;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
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
        private string ShowCaptchaSessionKey = "ShowCaptchaForHim";

        public AccountController(IMembershipUserService userHelper, ISystemMailer mailer, ILogger logger) : base(userHelper)
        {
            this.mailer = mailer;
            this.logger = logger;
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
            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.ShowCapcha = IsCaptchaPresentOnLoginPage(null);
            return this.View(new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RecaptchaControlMvc.CaptchaValidatorAttribute]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            var isCapchaPresentOnLoginPage = IsCaptchaPresentOnLoginPage(model.UserName);

            if (AppSettings.Instance.IsReCaptchaEnabled && isCapchaPresentOnLoginPage && !captchaValid)
            {
                StoreInvalidAttempt(model.UserName);
                this.ViewBag.ShowCapcha = true;
                this.Error(ErrorMessages.You_did_not_type_the_verification_word_correctly);
                return View(model);
            }

            if (this.ModelState.IsValid
                && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                if (model.RememberMe)
                {
                    Response.Cookies[0].Expires = DateTime.Now.AddDays(1);
                }

                ResetInvalidAttemptsCount(model.UserName);
                return this.RedirectToLocal(returnUrl);
            }

            StoreInvalidAttempt(model.UserName);

            this.ViewBag.ShowCapcha = IsCaptchaPresentOnLoginPage(model.UserName);
            this.Error(ErrorMessages.The_user_name_or_password_provided_is_incorrect);
            return View(model);
        }

        private void StoreInvalidAttempt(string userName)
        {
            var appSettings = AppSettings.Instance;
            Queue<DateTime> existingLoginAttempts = HttpContext.Cache.Get(userName) as Queue<DateTime>;
            if (existingLoginAttempts == null)
            {
                existingLoginAttempts = new Queue<DateTime>(appSettings.CountOfFailedLoginAttemptsBeforeCaptcha + 1);
            }

            existingLoginAttempts.Enqueue(DateTime.Now);

            var invalidAttemptsExceededLimit = existingLoginAttempts.Count >= appSettings.CountOfFailedLoginAttemptsBeforeCaptcha;
            var expire = DateTime.Now + appSettings.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt;

            if (invalidAttemptsExceededLimit)
            {
                Response.Cookies.Add(new HttpCookie(ShowCaptchaSessionKey, invalidAttemptsExceededLimit.ToString())
                {
                    Expires = expire
                });
            }

            HttpContext.Cache.Insert(userName,
                existingLoginAttempts,
                null,
                expire,
                System.Web.Caching.Cache.NoSlidingExpiration);
        }

        private void ResetInvalidAttemptsCount(string userName)
        {
            HttpContext.Cache.Remove(userName);
            if (Request.Cookies[ShowCaptchaSessionKey] != null)
            {
                Request.Cookies.Remove(ShowCaptchaSessionKey);
                var httpCookie = new HttpCookie(ShowCaptchaSessionKey);
                httpCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(httpCookie);
            }

            Session.Remove(ShowCaptchaSessionKey);
        }

        private bool IsCaptchaPresentOnLoginPage(string userName)
        {
            string showCaptchaForHim = Monads.Maybe(() => Request.Cookies[ShowCaptchaSessionKey].Value);
            if (!string.IsNullOrEmpty(showCaptchaForHim)) return true;
            if (string.IsNullOrEmpty(userName)) return false;

            Queue<DateTime> existingLoginAttempts = HttpContext.Cache.Get(userName) as Queue<DateTime>;

            if (existingLoginAttempts == null || existingLoginAttempts.Count < AppSettings.Instance.CountOfFailedLoginAttemptsBeforeCaptcha)
            {
                return false;
            }

            return true;
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
        public ActionResult PasswordReset(ResetPasswordModel model)
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

                    this.mailer.ResetPasswordEmail(
                        new EmailConfirmationModel()
                            {
                                Email = user.Email.ToWBEmailAddress(),
                                UserName = model.UserName,
                                ConfirmationToken = confirmationToken
                            }).SendAsync();

                    this.Error(ErrorMessages.Look_for_an_email_in_your_inbox);
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
        public ActionResult Register(RegisterModel model, bool captchaValid)
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

                            this.mailer.ConfirmationEmail(
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
        public ActionResult ResendConfirmation(string id)
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
                        this.mailer.ConfirmationEmail(
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index");
            }
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