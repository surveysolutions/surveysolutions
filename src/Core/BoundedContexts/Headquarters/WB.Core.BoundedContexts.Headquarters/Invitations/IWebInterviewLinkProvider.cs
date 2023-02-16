namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IWebInterviewLinkProvider
    {
        string WebInterviewStartLink(Invitation invitation);
        string WebInterviewContinueLink(Invitation invitation);
        string WebInterviewRequestLink(string assignmentId, string guid);
        string GetViewInBrowserLink(string modelId);
    }
}
