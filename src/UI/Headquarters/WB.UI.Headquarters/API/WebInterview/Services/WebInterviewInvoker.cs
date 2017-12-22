using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewInvoker : IWebInterviewInvoker
    {
        private readonly IHubConnectionContext<dynamic> hubContext;

        public WebInterviewInvoker(IHubConnectionContext<dynamic> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void RefreshEntities(string interviewId, string[] identities)
        {
            this.hubContext.Group(interviewId).refreshEntities(identities);
        }

        public void RefreshSection(Guid interviewId)
        {
            this.hubContext.Group(interviewId.FormatGuid()).refreshSection();
        }

        public void ReloadInterview(Guid interviewId)
        {
            this.hubContext.Group(interviewId.FormatGuid()).reloadInterview();
        }

        public void FinishInterview(Guid interviewId)
        {
            this.hubContext.Group(interviewId.FormatGuid()).finishInterview();
        }

        public void MarkAnswerAsNotSaved(string section, string questionId, string errorMessage)
        {
            this.hubContext.Group(section).markAnswerAsNotSaved(questionId, errorMessage);
        }

        public void ReloadInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            this.hubContext.Group(questionnaireIdentity.ToString()).reloadInterview();
        }
    }
}