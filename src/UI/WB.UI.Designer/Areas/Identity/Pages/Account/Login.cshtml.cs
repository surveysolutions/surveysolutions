using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<DesignerIdentityUser> signInManager;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(SignInManager<DesignerIdentityUser> signInManager, 
            UserManager<DesignerIdentityUser> userManager,
            ILogger<LoginModel> logger
            )
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public bool ShouldShowCaptcha { get; set; }

        public class InputModel
        {
            [Required(ErrorMessageResourceType = typeof(Resources.ErrorMessages), ErrorMessageResourceName = "UserName_required")]
            public string Email { get; set; }

            [Required(ErrorMessageResourceType = typeof(Resources.ErrorMessages), ErrorMessageResourceName = "Password_required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var user = await userManager.FindByNameAsync(Input.Email) ??
                           await userManager.FindByEmailAsync(Input.Email);

                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe,
                        lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
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
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
