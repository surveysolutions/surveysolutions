using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IWebInterviewEmailRenderer
    {
        Task<PersonalizedWebInterviewEmail> RenderEmail(WebInterviewEmailTemplate emailTemplate, string password, string link, string surveyName,
            string address, string senderName);
        Task<PersonalizedWebInterviewEmail> RenderEmail(EmailParams emailParams);
        Task<string> RenderHtmlEmail(EmailParams emailParams);
        Task<string> RenderTextEmail(EmailParams emailParams);
    }
}
