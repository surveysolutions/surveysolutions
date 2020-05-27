using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Enumerator.Native.WebInterview.Pipeline;

namespace WB.UI.Headquarters.Code.WebInterview.Pipeline
{
    public class PauseResumePipelineModule : IPipelineModule
    {
        private readonly IPauseResumeQueue pauseResumeQueue;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAggregateRootPrototypeService prototypeService;

        public PauseResumePipelineModule(IPauseResumeQueue pauseResumeQueue, IAuthorizedUser authorizedUser, IAggregateRootPrototypeService prototypeService)
        {
            this.pauseResumeQueue = pauseResumeQueue;
            this.authorizedUser = authorizedUser;
            this.prototypeService = prototypeService;
        }

        public Task OnConnected(Hub hub)
        {
            var interviewId = hub.GetInterviewId();

            if (prototypeService.IsPrototype(interviewId)) return Task.CompletedTask;

            if (authorizedUser.IsInterviewer)
            {
                pauseResumeQueue.EnqueueResume(new ResumeInterviewCommand(interviewId, this.authorizedUser.Id));
            }
            else if (authorizedUser.IsSupervisor)
            {
                pauseResumeQueue.EnqueueOpenBySupervisor(new OpenInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id));
            }

            return Task.CompletedTask;
        }

        public Task OnDisconnected(Hub hub, Exception exception)
        {
            var interviewId = hub.GetInterviewId();

            if (prototypeService.IsPrototype(interviewId)) return Task.CompletedTask;

            if (authorizedUser.IsInterviewer)
            {
                pauseResumeQueue.EnqueuePause(new PauseInterviewCommand(interviewId, this.authorizedUser.Id));
            }
            else if (authorizedUser.IsSupervisor)
            {
                pauseResumeQueue.EnqueueCloseBySupervisor(new CloseInterviewBySupervisorCommand(interviewId, this.authorizedUser.Id));
            }

            return Task.CompletedTask;
        }
    }
}
