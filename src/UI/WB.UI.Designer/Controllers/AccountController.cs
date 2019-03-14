using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICaptchaProtectedAuthenticationService captchaProtectedAuthentication;
        private readonly IOptions<CaptchaConfig> captchaOptions;
        private readonly ICaptchaService captchaService;
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IEmailSender emailSender;
        private readonly ILogger<AccountController> logger;

        public AccountController(ICaptchaProtectedAuthenticationService captchaProtectedAuthentication,
            IOptions<CaptchaConfig> captchaOptions,
            ICaptchaService captchaService,
            UserManager<DesignerIdentityUser> users,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            this.captchaProtectedAuthentication = captchaProtectedAuthentication;
            this.captchaOptions = captchaOptions;
            this.captchaService = captchaService;
            this.users = users;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [AllowAnonymous]
        [ResponseCache(NoStore = true)]
        public ActionResult Login(string returnUrl)
        {
            return this.View(new LogonModel
            {
                ShouldShowCaptcha = this.captchaProtectedAuthentication.ShouldShowCaptcha(),
                StaySignedIn = false
            });
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
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var isUserRegisterSuccessfully = false;
            if (this.captchaOptions.Value.IsReCaptchaEnabled && !IsCaptchaValid())
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
                        var designerIdentityUser = new DesignerIdentityUser
                        {
                            Id = providerUserKey.FormatGuid(),
                            UserName = userName,
                            NormalizedUserName = userName.ToUpperInvariant(),
                            Email = model.Email
                        };
                        var identityResult = await this.users.CreateAsync(designerIdentityUser);
                        if (identityResult.Succeeded)
                        {
                            var confirmationToken = await this.users.GenerateEmailConfirmationTokenAsync(designerIdentityUser);
                            //string confirmationToken = WebSecurity.CreateUserAndAccount(
                            //    userName, model.Password, new { Email = model.Email, FullName = model.FullName, ProviderUserKey = providerUserKey }, true);

                            if (!string.IsNullOrEmpty(confirmationToken))
                            {
                                await this.users.AddToRoleAsync(designerIdentityUser, SimpleRoleEnum.User.ToString());

                                isUserRegisterSuccessfully = true;
                                
                                //await this.emailSender.SendEmailAsync(model.Email,
                                //    NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process,
                                //    ) todo restore email
                                    
                                    //.mailer.ConfirmationEmail(
                                    //new EmailConfirmationModel()
                                    //{
                                    //    Email = model.Email.ToWBEmailAddress(),
                                    //    UserName = model.FullName ?? userName,
                                    //    ConfirmationToken = confirmationToken
                                    //}).SendAsync();
                            }

                        }
                        else
                        {
                            foreach (var identityResultError in identityResult.Errors)
                            {
                                this.Error(identityResultError.Description, true);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        logger.LogError("Register user error", e);
                        this.Error(ErrorMessages.Unexpected_error_occurred_Please_try_again_later);
                    }
                }
            }

            return isUserRegisterSuccessfully ? this.RegisterStepTwo() : this.View(model);
        }

        private bool IsCaptchaValid()
        {
            throw new NotImplementedException();
        }

        public IActionResult PasswordReset()
        {
            throw new System.NotImplementedException();
        }
    }
}
