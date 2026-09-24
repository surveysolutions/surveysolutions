using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Identity.Pages.Account
{
    public class ResendConfirmationLinkModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IViewRenderService viewRenderingService;
        private readonly IEmailSender emailSender;
        private readonly ILogger<ResendConfirmationLinkModel> logger;

        public ResendConfirmationLinkModel(UserManager<DesignerIdentityUser> users,
            IViewRenderService viewRenderingService, 
            IEmailSender emailSender,
            ILogger<ResendConfirmationLinkModel> logger)
        {
            this.users = users ?? throw new ArgumentNullException(nameof(users));
            this.viewRenderingService = viewRenderingService ?? throw new ArgumentNullException(nameof(viewRenderingService));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await this.users.FindByNameOrEmailAsync(id);

            if (user != null)
            {
                var claims = await this.users.GetClaimsAsync(user);
                var existingFullName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

                var code = await this.users.GenerateEmailConfirmationTokenAsync(user);
                var model = new EmailConfirmationModel();
                model.UserName = !string.IsNullOrEmpty(existingFullName?.Value) ? existingFullName?.Value: user.UserName;
                model.ConfirmationLink = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new {userId = user.Id, code = code},
                    protocol: Request.Scheme);

                var messageBody =
                    await viewRenderingService.RenderToStringAsync("Emails/ConfirmationEmail",
                        model);

                if (user.Email != null)
                {
                    try
                    {
                        await emailSender.SendEmailAsync(user.Email,
                            NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process,
                            messageBody);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to resend confirmation email to user {UserId}", user.Id);
                    }
                }
            }

            return Redirect("./RegisterStepTwo");
        }
    }
}
