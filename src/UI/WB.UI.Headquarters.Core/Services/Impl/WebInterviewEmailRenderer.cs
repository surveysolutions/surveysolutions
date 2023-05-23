using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    public class WebInterviewEmailRenderer : IWebInterviewEmailRenderer
    {
        private readonly IOptions<HeadquartersConfig> options;
        private readonly IViewRenderService viewRenderService;

        public WebInterviewEmailRenderer(IViewRenderService viewRenderService,
            IOptions<HeadquartersConfig> options)
        {
            this.viewRenderService = viewRenderService;
            this.options = options;
        }

        public Task<PersonalizedWebInterviewEmail> RenderEmail(WebInterviewEmailTemplate emailTemplate, string password,
            string link,
            string surveyName, string address, string senderName)
        {
            var emailContent = new EmailContent(emailTemplate, surveyName, link, password);

            var model = new EmailParams
            {
                Subject = emailContent.Subject,
                LinkText = emailContent.LinkText,
                MainText = emailContent.MainText,
                PasswordDescription = emailContent.PasswordDescription,
                Password = password,
                Address = address,
                SurveyName = surveyName,
                SenderName = senderName,
                Link = link
            };

            return RenderEmailWithControllerContext(model);
        }

        public Task<PersonalizedWebInterviewEmail> RenderEmail(EmailParams emailParams)
        {
            return RenderEmailWithControllerContext(emailParams);
        }

        public Task<string> RenderHtmlEmail(EmailParams emailParams)
        {
            return this.viewRenderService.RenderToStringAsync("/Views/WebEmails/EmailHtml.cshtml", emailParams, options.Value.BaseUrl, options.Value.BaseAppUrl);
        }

        public Task<string> RenderTextEmail(EmailParams emailParams)
        {
            return this.viewRenderService.RenderToStringAsync("/Views/WebEmails/EmailText.cshtml", emailParams, options.Value.BaseUrl, options.Value.BaseAppUrl);
        }

        private async Task<PersonalizedWebInterviewEmail> RenderEmailWithControllerContext(EmailParams emailParams)
        {
            string html = await this.viewRenderService.RenderToStringAsync("/Views/WebEmails/EmailHtml.cshtml", emailParams, options.Value.BaseUrl, options.Value.BaseAppUrl);
            string text = await this.viewRenderService.RenderToStringAsync("/Views/WebEmails/EmailText.cshtml", emailParams, options.Value.BaseUrl, options.Value.BaseAppUrl);
            return new PersonalizedWebInterviewEmail(emailParams.Subject, html, text);
        }
    }
}
