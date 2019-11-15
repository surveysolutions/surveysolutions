using System;
using Microsoft.AspNetCore.SignalR;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewInvoker : IWebInterviewInvoker
    {
        private readonly Lazy<IHubContext<WebInterview>> lazyHubContext;

        public WebInterviewInvoker(Lazy<IHubContext<WebInterview>> lazyHubContext)
        {
            this.lazyHubContext = lazyHubContext;
        }

        private IHubContext<WebInterview> HubClients => lazyHubContext.Value;

        public void RefreshEntities(string interviewId, string[] identities)
        {
            this.HubClients.Clients.Group(interviewId).SendAsync("refreshEntities", identities);
        }

        public void RefreshSection(Guid interviewId)
        {
            this.HubClients.Clients.Group(interviewId.FormatGuid()).SendAsync("refreshSection", Array.Empty<object>());
        }

        public void RefreshSectionState(Guid interviewId)
        {
            this.HubClients.Clients.Group(interviewId.FormatGuid()).SendAsync("refreshSectionState", Array.Empty<object>());
        }

        public void ReloadInterview(Guid interviewId)
        {
            this.HubClients.Clients.Group(interviewId.FormatGuid()).SendAsync("reloadInterview", Array.Empty<object>());
        }

        public void FinishInterview(Guid interviewId)
        {
            this.HubClients.Clients.Group(interviewId.FormatGuid()).SendAsync("finishInterview", Array.Empty<object>());
        }

        public void MarkAnswerAsNotSaved(string section, string questionId, string errorMessage)
        {
            this.HubClients.Clients.Group(section).SendAsync("markAnswerAsNotSaved", questionId, errorMessage);
        }

        public void ShutDown(Guid interviewId)
        {
            this.HubClients.Clients.Group(interviewId.FormatGuid()).SendAsync("shutDown", Array.Empty<object>());
        }
    }
}
