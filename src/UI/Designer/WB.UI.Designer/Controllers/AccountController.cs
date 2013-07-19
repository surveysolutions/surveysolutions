﻿using WB.Core.GenericSubdomains.Logging;
using WB.UI.Designer.Code;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Security;
    using System.Web.UI;

    using Main.Core.Utility;

    using Recaptcha;

    using WB.UI.Designer.Extensions;
    using WB.UI.Designer.Mailers;
    using WB.UI.Designer.Models;
    using WB.UI.Shared.Web.Membership;

    using WebMatrix.WebData;

    [CustomAuthorize]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
    [RequireHttps]
    public class AccountController : BaseController
    {
        private readonly ISystemMailer mailer;
        private readonly ILogger logger;

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
            return this.View(new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None", Location = OutputCacheLocation.None)]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (this.ModelState.IsValid
                && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return this.RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            this.Error("The user name or password provided is incorrect.");
            return View(model);
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
                    // Success();
                    return this.RedirectToAction(
                        "manage", new { message = AccountManageMessageId.ChangePasswordSuccess });
                }
                else
                {
                    this.Error("The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

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
                    this.Error(string.Format("User {0} does not exist. Please enter a valid user name", model.UserName));
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

                    this.Attention("To complete the reset password process look for an email in your inbox that provides further instructions.");
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
                this.Error("You did not type the verification word correctly. Please try again.");
            }
            else
            {
                if (this.ModelState.IsValid)
                {

                    // Attempt to register the user
                    try
                    {
                        string confirmationToken = WebSecurity.CreateUserAndAccount(
                            model.UserName, model.Password, new { model.Email }, true);

                        if (!string.IsNullOrEmpty(confirmationToken))
                        {
                            Roles.Provider.AddUsersToRoles(new[] { model.UserName }, new[] { this.UserHelper.USERROLENAME });

                            isUserRegisterSuccessfully = true;

                            this.mailer.ConfirmationEmail(
                                new EmailConfirmationModel()
                                    {
                                        Email = model.Email.ToWBEmailAddress(),
                                        UserName = model.UserName,
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
                        logger.Error("Unexpected error occurred", e);
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
                this.Success("You have completed the registration process. You can now logon to the system");
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
                        this.Error("Unexpected problem. Contact with administrator to solve this problem.");
                    }
                }
                else
                {
                    this.Error(string.Format("User {0} already confirmed in system.", model.UserName));
                }
            }
            else
            {
                this.Error(string.Format("User {0} does not exist. Please enter a valid user name.", id));
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
                    this.Success("Your password successfully changed. Now you can login with your new password");
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
            this.Attention(
                "To complete the registration process look for an email in your inbox that provides further instructions.");
            return this.RedirectToAction("Login");
        }
    }
}