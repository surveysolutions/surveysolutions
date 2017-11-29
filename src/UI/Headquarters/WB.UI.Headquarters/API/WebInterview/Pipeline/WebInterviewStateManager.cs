using System;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        private readonly IProductVersion productVersion;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewStateManager(IProductVersion productVersion, 
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.productVersion = productVersion;
            this.statefulInterviewRepository = statefulInterviewRepository;
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
                    ? WebInterview.GetConnectedClientPrefilledSectionKey(interviewId) 
                    : WebInterview.GetConnectedClientSectionKey(sectionId, interviewId));

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
            var isInterviewer = hub.Context.User.IsInRole(UserRoles.Interviewer.ToString());
            var isSupervisor = hub.Context.User.IsInRole(UserRoles.Supervisor.ToString());

            var interviewId = hub.Context.QueryString[@"interviewId"];
            var interview = this.statefulInterviewRepository.Get(interviewId);
            Guid userId = Guid.Parse(hub.Context.User.Identity.GetUserId());

            ICommand pauseInterviewCommand = null;
            if (isInterviewer && !interview.IsCompleted)
            {
                pauseInterviewCommand = new PauseInterviewCommand(Guid.Parse(interviewId), userId, DateTime.Now, DateTime.UtcNow);
            }
            else if (isSupervisor && interview.Status != InterviewStatus.ApprovedBySupervisor)
            {
                pauseInterviewCommand =
                    new CloseInterviewBySupervisorCommand(Guid.Parse(interviewId), userId, DateTime.Now);
            }

            if (pauseInterviewCommand != null)
            {
                // There is no request scope so no other way to get scoped transaction
                var transactionManager = ServiceLocator.Current.GetInstance<ITransactionManagerProvider>()
                    .GetTransactionManager();
                try
                {
                    transactionManager.BeginCommandTransaction();
                    ServiceLocator.Current.GetInstance<ICommandService>().Execute(pauseInterviewCommand);
                    transactionManager.CommitCommandTransaction();
                }
                catch (Exception)
                {
                    transactionManager.RollbackCommandTransaction();
                    throw;
                }
            }
        }
    }
}