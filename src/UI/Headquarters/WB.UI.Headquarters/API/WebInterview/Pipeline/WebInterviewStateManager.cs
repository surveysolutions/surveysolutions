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
    public class WebInterviewStateManager : HubPipelineModule
    {
        private readonly IProductVersion productVersion;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IPauseResumeQueue pauseResumeQueue;

        public WebInterviewStateManager(IProductVersion productVersion, 
            IStatefulInterviewRepository statefulInterviewRepository,
            IPauseResumeQueue pauseResumeQueue)
        {
            this.productVersion = productVersion;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.pauseResumeQueue = pauseResumeQueue;
        }

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterConnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString[@"interviewId"];
            var interview = this.statefulInterviewRepository.Get(interviewId);
            var isReview = hub.Context.QueryString[@"review"].ToBool(false);

            if (!isReview)
            {
                hub.Clients.OthersInGroup(interviewId).closeInterview();
            }

            hub.Groups.Add(hub.Context.ConnectionId, interview.QuestionnaireIdentity.ToString());

            base.OnAfterConnect(hub);
        }

        protected override bool OnBeforeReconnect(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return base.OnBeforeReconnect(hub);
        }

        private void ReloadIfOlderVersion(IHub hub)
        {
            if (this.productVersion.ToString() != hub.Context.QueryString[@"appVersion"])
            {
                hub.Clients.Caller.reloadInterview();
            }
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Context.QueryString[@"interviewId"];
            var sectionId = context.Hub.Clients.CallerState.sectionId as string;

            if (interviewId != null)
            {
                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, 
                    sectionId == null 
                    ? Enumerator.Native.WebInterview.WebInterview.GetConnectedClientPrefilledSectionKey(Guid.Parse(interviewId)) 
                    : Enumerator.Native.WebInterview.WebInterview.GetConnectedClientSectionKey(Identity.Parse(sectionId), Guid.Parse(interviewId)));

                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, interviewId);
            }

            return base.OnAfterIncoming(result, context);
        }

        protected override bool OnBeforeDisconnect(IHub hub, bool stopCalled)
        {
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
                pauseResumeQueue.EnqueuePause(new PauseInterviewCommand(Guid.Parse(interviewId), userId, DateTime.Now, DateTime.UtcNow));
            }
            else if (isSupervisor && interview.Status != InterviewStatus.ApprovedBySupervisor)
            {
                pauseResumeQueue.EnqueueCloseBySupervisor(new CloseInterviewBySupervisorCommand(Guid.Parse(interviewId), userId, DateTime.Now));
            }
        }
    }
}