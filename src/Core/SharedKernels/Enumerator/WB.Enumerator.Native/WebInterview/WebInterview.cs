﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.Enumerator.Native.WebInterview
{
    public class WebInterview : Hub
    {
        private const string SectionId = "sectionId";

        private readonly IPipelineModule[] hubPipelineModules;

        private string CallerInterviewId
        {
            get
            {
                var http = this.Context.GetHttpContext();
                return http.Request.Query["interviewId"];
            }
        }

        public WebInterview(IPipelineModule[] hubPipelineModules)
        {
            this.hubPipelineModules = hubPipelineModules;
        }

        public override async Task OnConnectedAsync()
        {
            await RegisterClient();

            foreach (var pipelineModule in hubPipelineModules)
            {
                await pipelineModule.OnConnected(this);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var pipelineModule in hubPipelineModules)
            {
                await pipelineModule.OnDisconnected(this, exception);
            }
            
            await UnRegisterClient();

            await base.OnDisconnectedAsync(exception);
        }

        private async Task RegisterClient()
        {
            var interviewId = CallerInterviewId;
            await Groups.AddToGroupAsync(Context.ConnectionId, interviewId);
            await Clients.OthersInGroup(interviewId).SendAsync("closeInterview");
        }

        private async Task UnRegisterClient()
        {
            var interviewId = CallerInterviewId;
            var sectionId = this.Context.Items[SectionId] as string;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupNameBySectionIdentity(sectionId, interviewId));
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, interviewId);
        }
        
        public async Task ChangeSection(string sectionId, string oldSectionId = null)
        {
            var interviewId = CallerInterviewId;
            var oldSection = this.Context.Items[SectionId] as string;

            if (interviewId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupNameBySectionIdentity(oldSection, interviewId));
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupNameBySectionIdentity(sectionId, interviewId));
            }
        }

        private static string GetGroupNameBySectionIdentity(string sectionId, string interviewId)
        {
            return sectionId == null
                ? GetConnectedClientPrefilledSectionKey(Guid.Parse(interviewId))
                : GetConnectedClientSectionKey(Identity.Parse(sectionId), Guid.Parse(interviewId));
        }


        [Localizable(false)]
        public static string GetConnectedClientSectionKey(Identity sectionId, Guid interviewId) => $"{sectionId}x{interviewId}";

        [Localizable(false)]
        public static string GetConnectedClientPrefilledSectionKey(Guid interviewId) => $"PrefilledSectionx{interviewId}";
        

        public static string GetUiMessageFromException(Exception e)
        {
            if (e is InterviewException interviewException && interviewException.ExceptionType != InterviewDomainExceptionType.Undefined)
            {
                switch (interviewException.ExceptionType)
                {
                    case InterviewDomainExceptionType.InterviewLimitReached:
                        return Enumerator.Native.Resources.WebInterview.ServerUnderLoad;
                    case InterviewDomainExceptionType.QuestionnaireIsMissing:
                    case InterviewDomainExceptionType.InterviewHardDeleted:
                        return Enumerator.Native.Resources.WebInterview.Error_InterviewExpired;
                    case InterviewDomainExceptionType.OtherUserIsResponsible:
                    case InterviewDomainExceptionType.StatusIsNotOneOfExpected:
                        return Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded;
                    case InterviewDomainExceptionType.InterviewRecievedByDevice:
                        return Enumerator.Native.Resources.WebInterview.InterviewReceivedByInterviewer;
                    case InterviewDomainExceptionType.InterviewSizeLimitReached:
                        return Enumerator.Native.Resources.WebInterview.InterviewSizeLimitReached;
                }
            }

            return e.Message;
        }
    }
}
