using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        public IOptions<CaptchaConfig> CaptchaOptions { get; }

        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly IViewRenderService viewRenderingService;
        private readonly ILogger<RegisterModel> logger;
        private readonly IEmailSender emailSender;
        private readonly IRecaptchaService recaptchaService;

        public RegisterModel(
            UserManager<DesignerIdentityUser> userManager,
            IViewRenderService viewRenderingService,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IRecaptchaService recaptchaService,
            IOptions<CaptchaConfig> captchaOptions)
        {
            this.CaptchaOptions = captchaOptions;
            this.userManager = userManager;
            this.viewRenderingService = viewRenderingService;
            this.logger = logger;
            this.emailSender = emailSender;
            this.recaptchaService = recaptchaService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "UserName_required")]
            [Display(Name = "User name", Order = 1)]
            [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "User_name_needs_to_be_between_3_and_15_characters")]
            public string? Login { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Email_required))]
            [EmailAddress(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.InvalidEmailAddress))]
            public string? Email { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Password_required))]
            [DataType(DataType.Password)]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.The_password_and_confirmation_password_do_not_match))]
            public string? ConfirmPassword { get; set; }

            [Display(Name = "Full name", Order = 4)]
            [StringLength(100, ErrorMessageResourceName = nameof(ErrorMessages.FullNameMaxLengthError), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string? FullName { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : null;
            if (Input != null && ModelState.IsValid)
            {
                if (this.CaptchaOptions.Value.CaptchaType == CaptchaProviderType.Recaptcha)
                {
                    var recaptcha = await this.recaptchaService.Validate(Request);
                    if (!recaptcha.success)
                    {
                        this.ErrorMessage = ErrorMessages.You_did_not_type_the_verification_word_correctly;
                        return Page();
                    }
                }

                var user = new DesignerIdentityUser
                {
                    UserName = Input.Login,
                    Email = Input.Email,
                    CreatedAtUtc = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    logger.LogInformation("User created a new account with password.");

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    if (!string.IsNullOrWhiteSpace(Input.FullName))
                    {
                        await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, Input.FullName));
                    }

                    await userManager.AddToRoleAsync(user, SimpleRoleEnum.User.ToString());

                    var model = new EmailConfirmationModel();
                    model.UserName = !string.IsNullOrWhiteSpace(Input.FullName) ? Input.FullName : Input.Login;
                    model.ConfirmationLink = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    var messageBody =
                        await viewRenderingService.RenderToStringAsync("Emails/ConfirmationEmail",
                            model);

                    await emailSender.SendEmailAsync(Input.Email,
                        NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process,
                        messageBody);

                    return RedirectToPage("RegisterStepTwo", new { returnUrl });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
