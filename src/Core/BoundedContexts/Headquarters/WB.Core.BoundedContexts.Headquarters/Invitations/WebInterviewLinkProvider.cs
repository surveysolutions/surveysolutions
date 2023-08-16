﻿using WB.UI.Shared.Web.Services;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class WebInterviewLinkProvider : IWebInterviewLinkProvider
    {
        private readonly IVirtualPathService pathService;
        
        public WebInterviewLinkProvider(IVirtualPathService pathService)
        {
            this.pathService = pathService;
        }

        private string WebInterviewStartPath(Invitation invitation)
            => $"~/WebInterview/{invitation.Token}/Start";

        private string WebInterviewContinuePath(Invitation invitation)
            => $"~/WebInterview/Continue/{invitation.Token}";

        public string WebInterviewStartLink(Invitation invitation)
        {
            return pathService.GetAbsolutePath(WebInterviewStartPath(invitation));
        }

        public string WebInterviewContinueLink(Invitation invitation)
        {
            return pathService.GetAbsolutePath(WebInterviewContinuePath(invitation));
        }

        public string WebInterviewRequestLink(string assignmentId, string guid)
        {
            return pathService.GetAbsolutePath($"~/webinterview/link/{assignmentId}/{guid}");
        }

        public string GetViewInBrowserLink(string modelId)
        {
            return pathService.GetAbsolutePath($"~/WebEmails/Html/{modelId}");
        }
    }
}
