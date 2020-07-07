using Microsoft.Extensions.Options;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewLinkProvider : IWebInterviewLinkProvider
    {
        private readonly IOptions<HeadquartersConfig> headquartersOptions;

        public WebInterviewLinkProvider(IOptions<HeadquartersConfig> headquartersOptions)
        {
            this.headquartersOptions = headquartersOptions;
        }

        string WebInterviewStartPath(Invitation invitation)
            => $"/WebInterview/{invitation.Token}/Start";

        string WebInterviewContinuePath(Invitation invitation)
            => $"/WebInterview/Continue/{invitation.Token}";

        public string WebInterviewStartLink(Invitation invitation)
        {
            return headquartersOptions.Value.BaseUrl.TrimEnd('/')
                   + WebInterviewStartPath(invitation);
        }

        public string WebInterviewContinueLink(Invitation invitation)
        {
            return headquartersOptions.Value.BaseUrl.TrimEnd('/')
                   + WebInterviewContinuePath(invitation);
        }
    }
}
