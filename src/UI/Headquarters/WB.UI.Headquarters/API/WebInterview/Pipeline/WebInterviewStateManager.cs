using System;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class HandlePauseEventPipelineModule : HubPipelineModule
    {
        private readonly IPauseResumeQueue pauseResumeQueue;
        private IServiceLocator serviceLocator;

        public HandlePauseEventPipelineModule(IPauseResumeQueue pauseResumeQueue, IServiceLocator serviceLocator)
        {
            this.pauseResumeQueue = pauseResumeQueue ?? throw new ArgumentNullException(nameof(pauseResumeQueue));
            this.serviceLocator = serviceLocator;
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

            //resolve from context to preserve scope
            IStatefulInterview interview = null;
            this.serviceLocator.ExecuteActionInScope((locator) =>
            {
                IStatefulInterviewRepository statefulInterviewRepository = locator.GetInstance<IStatefulInterviewRepository>();
                interview = statefulInterviewRepository.Get(interviewId);
            });
            
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
