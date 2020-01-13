using WB.Core.BoundedContexts.Headquarters.Invitations;

namespace WB.UI.Headquarters.Services.Impl
{
    public class StubWebInterviewEmailRenderer : IWebInterviewEmailRenderer
    {
        public PersonalizedWebInterviewEmail RenderEmail(WebInterviewEmailTemplate emailTemplate, string password, string link,
            string surveyName, string address, string senderName)
        {
            throw new System.NotImplementedException();
        }

        public PersonalizedWebInterviewEmail RenderEmail(EmailParams emailParams)
        {
            throw new System.NotImplementedException();
        }
    }
}
