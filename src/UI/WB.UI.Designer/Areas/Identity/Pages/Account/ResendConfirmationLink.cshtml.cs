using System;
using System.Threading.Tasks;
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
    public class ResendConfirmationLinkModel : PageModel
    {
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IViewRenderingService viewRenderingService;
        private readonly IEmailSender emailSender;

        public ResendConfirmationLinkModel(UserManager<DesignerIdentityUser> users,
            IViewRenderingService viewRenderingService, 
            IEmailSender emailSender)
        {
            this.users = users ?? throw new ArgumentNullException(nameof(users));
            this.viewRenderingService = viewRenderingService ?? throw new ArgumentNullException(nameof(viewRenderingService));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await this.users.FindByNameOrEmailAsync(id);

            if (user != null)
            {
                var code = await this.users.GenerateEmailConfirmationTokenAsync(user);
                var model = new EmailConfirmationModel();
                model.UserName = user.UserName;
                model.ConfirmationLink = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new {userId = user.Id, code = code},
                    protocol: Request.Scheme);

                var messageBody =
                    await viewRenderingService.RenderToStringAsync("Emails/ConfirmationEmail",
                        model);

                await emailSender.SendEmailAsync(user.Email,
                    NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process,
                    messageBody);
            }

            return Redirect("./RegisterStepTwo");
        }
    }
}
