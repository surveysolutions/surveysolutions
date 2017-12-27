using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewInvoker : IWebInterviewInvoker
    {
        private readonly Lazy<IHubConnectionContext<dynamic>> lazyHubContext;

        public WebInterviewInvoker(Lazy<IHubConnectionContext<dynamic>> lazyHubContext)
        {
            this.lazyHubContext = lazyHubContext;
        }

        private IHubConnectionContext<dynamic> HubClients => lazyHubContext.Value;

        public void RefreshEntities(string interviewId, string[] identities)
        {
            this.HubClients.Group(interviewId).refreshEntities(identities);
        }

        public void RefreshSection(Guid interviewId)
        {
            this.HubClients.Group(interviewId.FormatGuid()).refreshSection();
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

        public void ReloadInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            this.HubClients.Group(questionnaireIdentity.ToString()).reloadInterview();
        }
    }
}