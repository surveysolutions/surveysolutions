using System;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class HandlePauseEventPipelineModule : HubPipelineModule
    {
        private readonly IPauseResumeQueue pauseResumeQueue;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public HandlePauseEventPipelineModule(IPauseResumeQueue pauseResumeQueue, IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.pauseResumeQueue = pauseResumeQueue ?? throw new ArgumentNullException(nameof(pauseResumeQueue));
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
        }

        protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
        {
            RecordInterviewPause(hub);
            return base.OnBeforeDisconnect(hub, stopCalled);
        }

        private void RecordInterviewPause(IHub hub)
        {
            var isAuthenticated = hub.Context.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
                return;

            var isInterviewer = hub.Context.User.IsInRole(UserRoles.Interviewer.ToString());
            var isSupervisor = hub.Context.User.IsInRole(UserRoles.Supervisor.ToString());

            var interviewId = hub.Context.QueryString[@"interviewId"];
            var interview = this.statefulInterviewRepository.Get(interviewId);
            Guid userId = Guid.Parse(hub.Context.User.Identity.GetUserId());

            if (isInterviewer && !interview.IsCompleted)
            {
                pauseResumeQueue.EnqueuePause(new PauseInterviewCommand(Guid.Parse(interviewId), userId));
            }
            else if (isSupervisor && interview.Status != InterviewStatus.ApprovedBySupervisor)
            {
                pauseResumeQueue.EnqueueCloseBySupervisor(new CloseInterviewBySupervisorCommand(Guid.Parse(interviewId), userId));
            }
        }
    }
}
