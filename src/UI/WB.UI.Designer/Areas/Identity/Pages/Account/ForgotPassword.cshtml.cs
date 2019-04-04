using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly IViewRenderingService viewRenderingService;

        public ForgotPasswordModel(UserManager<DesignerIdentityUser> userManager, 
            IEmailSender emailSender, 
            IViewRenderingService viewRenderingService)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.viewRenderingService = viewRenderingService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email)
                    ?? await userManager.FindByNameAsync(Input.Email);

                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: Request.Scheme);

                EmailConfirmationModel emailModel = new EmailConfirmationModel();
                emailModel.ConfirmationLink = callbackUrl;
                emailModel.UserName = user.UserName;
                string body = await this.viewRenderingService.RenderToStringAsync("Emails/ResetPasswordEmail", emailModel);
                
                await emailSender.SendEmailAsync(
                    Input.Email,
                    AccountResources.PasswordReset,
                    body);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
