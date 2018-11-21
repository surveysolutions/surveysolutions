using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        private readonly IProductVersion productVersion;
        private readonly IStatefulInterviewRepository interviewRepository;

        public WebInterviewStateManager(IProductVersion productVersion, 
            IStatefulInterviewRepository interviewRepository)
        {
            this.productVersion = productVersion;
            this.interviewRepository = interviewRepository;
        }

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterConnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString[@"interviewId"];
            IStatefulInterview interview = null;

            interview = interviewRepository.Get(interviewId);

            if (interview == null)
            {
                hub.Clients.Caller.shutDown();
                return;
            }

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
    }
}
