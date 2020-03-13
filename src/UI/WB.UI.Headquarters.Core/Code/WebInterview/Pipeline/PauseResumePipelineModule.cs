using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.UI.Headquarters.Code.WebInterview.Pipeline
{
    public class PauseResumePipelineModule : IPipelineModule
    {
        private readonly IPauseResumeQueue pauseResumeQueue;
        private readonly IAuthorizedUser authorizedUser;

        public PauseResumePipelineModule(IPauseResumeQueue pauseResumeQueue, IAuthorizedUser authorizedUser)
        {
            this.pauseResumeQueue = pauseResumeQueue;
            this.authorizedUser = authorizedUser;
        }

        public Task OnConnected(Hub hub)
        {
            var interviewId = GetInterviewId(hub);
            Guid interviewGuid = Guid.Parse(interviewId);
            if (authorizedUser.IsInterviewer)
            {
                pauseResumeQueue.EnqueueResume(new ResumeInterviewCommand(interviewGuid, this.authorizedUser.Id));
            }
            else if (authorizedUser.IsSupervisor)
            {
                pauseResumeQueue.EnqueueOpenBySupervisor(new OpenInterviewBySupervisorCommand(interviewGuid, this.authorizedUser.Id));
            }

            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            var interviewId = GetInterviewId(hub);
            Guid interviewGuid = Guid.Parse(interviewId);
            if (authorizedUser.IsInterviewer)
            {
                pauseResumeQueue.EnqueuePause(new PauseInterviewCommand(interviewGuid, this.authorizedUser.Id));
            }
            else if (authorizedUser.IsSupervisor)
            {
                pauseResumeQueue.EnqueueCloseBySupervisor(new CloseInterviewBySupervisorCommand(interviewGuid, this.authorizedUser.Id));
            }

            return Task.CompletedTask;
        }

        private string GetInterviewId(Hub hub)
        {
            var http = hub.Context.GetHttpContext();
            return http.Request.Query["interviewId"];
        }
    }
}
