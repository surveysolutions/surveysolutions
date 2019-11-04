using System;
using Microsoft.AspNetCore.SignalR;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewInvoker : IWebInterviewInvoker
    {
        private readonly Lazy<IHubClients> lazyHubContext;

        public WebInterviewInvoker(Lazy<IHubClients> lazyHubContext)
        {
            this.lazyHubContext = lazyHubContext;
        }

        private IHubClients<dynamic> HubClients => (IHubClients<dynamic>)lazyHubContext.Value;

        public void RefreshEntities(string interviewId, string[] identities)
        {
            this.HubClients.Group(interviewId).refreshEntities(identities);
        }

        public void RefreshSection(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).refreshSection();
        }

        public void RefreshSectionState(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).refreshSectionState();
        }

        public void ReloadInterview(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).reloadInterview();
        }

        public void FinishInterview(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).finishInterview();
        }

        public void MarkAnswerAsNotSaved(string section, string questionId, string errorMessage)
        {
            this.HubClients.Group(section).markAnswerAsNotSaved(questionId, errorMessage);
        }

        public void ShutDown(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).shutDown();
        }
    }
}
