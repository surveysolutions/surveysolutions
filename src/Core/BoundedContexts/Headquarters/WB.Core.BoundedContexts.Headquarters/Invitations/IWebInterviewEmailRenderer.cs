namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IWebInterviewEmailRenderer
    {
        PersonalizedWebInterviewEmail RenderEmail(WebInterviewEmailTemplate emailTemplate, string password, string link, string surveyName, string address, string senderName);
    }
}
