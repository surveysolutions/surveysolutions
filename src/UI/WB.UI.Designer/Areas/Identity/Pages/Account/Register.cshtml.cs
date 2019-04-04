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
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        public IOptions<CaptchaConfig> CaptchaOptions { get; }

        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly IViewRenderingService viewRenderingService;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<DesignerIdentityUser> userManager,
            IViewRenderingService viewRenderingService,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<CaptchaConfig> captchaOptions)
        {
            CaptchaOptions = captchaOptions;
            this.userManager = userManager;
            this.viewRenderingService = viewRenderingService;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "UserName_required")]
            [Display(Name = "User name", Order = 1)]
            [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = "User_name_needs_to_be_between_3_and_15_characters")]
            public string Login { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Email_required))]
            [EmailAddress(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.InvalidEmailAddress))]
            public string Email { get; set; }

            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Password_required))]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.The_password_and_confirmation_password_do_not_match))]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Full name", Order = 4)]
            [StringLength(100, ErrorMessageResourceName = nameof(ErrorMessages.FullNameMaxLengthError), ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessage = null)]
            public string FullName { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new DesignerIdentityUser { UserName = Input.Login, Email = Input.Email };
                var result = await userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    if (!string.IsNullOrWhiteSpace(Input.FullName))
                    {
                        await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, Input.FullName));
                    }

                    var model = new EmailConfirmationModel();
                    model.UserName = Input.Login;
                    model.ConfirmationLink = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme); 

                    var messageBody =
                        await viewRenderingService.RenderToStringAsync("Emails/ConfirmationEmail",
                            model);

                    await _emailSender.SendEmailAsync(Input.Email, 
                        NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process, 
                        messageBody);

                    return RedirectToPage("RegisterStepTwo", new {returnUrl});
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
