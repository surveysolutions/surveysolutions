using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<DesignerIdentityUser> signInManager;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly ILogger<LoginModel> logger;
        private readonly ICaptchaService captchaService;
        private readonly IRecaptchaService recaptchaService;

        public LoginModel(SignInManager<DesignerIdentityUser> signInManager, 
            UserManager<DesignerIdentityUser> userManager,
            ILogger<LoginModel> logger,
            ICaptchaService captchaService,
            IRecaptchaService recaptchaService
            )
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.captchaService = captchaService;
            this.recaptchaService = recaptchaService;
        }

        [BindProperty] 
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public bool ShouldShowCaptcha { get; set; }

        public bool ShowActivationLink { get; set; }

        public class InputModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Email_required))]
            public string? Email { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Password_required))]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            this.ShouldShowCaptcha = this.captchaService.ShouldShowCaptcha(null);

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Content("~/");

            if (Input != null)
            {
                this.ShouldShowCaptcha = this.captchaService.ShouldShowCaptcha(Input.Email);
                if (ModelState.IsValid)
                {
                    if (this.ShouldShowCaptcha)
                    {
                        try
                        {
                            var recaptcha = await this.recaptchaService.Validate(Request);
                            if (!recaptcha.success)
                            {
                                this.ErrorMessage = ErrorMessages.You_did_not_type_the_verification_word_correctly;
                                return Page();
                            }
                        }
                        catch (ValidationException)
                        {
                            this.ErrorMessage = ErrorMessages.You_did_not_type_the_verification_word_correctly;
                            return Page();
                        }
                    }

                    var user = await userManager.FindByNameOrEmailAsync(Input.Email);

                    if (user != null)
                    {
                        if (!user.EmailConfirmed)
                        {
                            this.ShowActivationLink = true;
                            this.ErrorMessage = string.Format(ErrorMessages.ConfirmAccount, user.Email);
                            return Page();
                        }

                        var result = await signInManager.PasswordSignInAsync(user,
                            Input.Password,
                            Input.RememberMe,
                            lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            this.captchaService.ResetFailedLogin(Input.Email);
                            logger.LogInformation("User logged in.");
                            return LocalRedirect(returnUrl);
                        }

                        if (result.RequiresTwoFactor)
                        {
                            return RedirectToPage("./LoginWith2fa",
                                new {ReturnUrl = returnUrl, RememberMe = Input.RememberMe});
                        }

                        if (result.IsLockedOut)
                        {
                            logger.LogWarning("User account locked out.");
                            return RedirectToPage("./Lockout");
                        }


                        this.captchaService.RegisterFailedLogin(Input.Email);
                        this.ErrorMessage = AccountResources.InvalidPassword;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

    }
}
