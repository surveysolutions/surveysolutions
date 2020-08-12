namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IWebInterviewLinkProvider
    {
        string WebInterviewStartLink(Invitation invitation);
        string WebInterviewContinueLink(Invitation invitation);
    }
}
