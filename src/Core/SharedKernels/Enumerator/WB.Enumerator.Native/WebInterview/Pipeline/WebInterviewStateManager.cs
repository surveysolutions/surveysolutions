using System;
using System.Web;
using Autofac;
using Autofac.Integration.Owin;
using Microsoft.AspNet.SignalR;
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

        public WebInterviewStateManager(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterConnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString[@"interviewId"];

            var autofacLifetimeScope = hub.Context.Request.GetHttpContext().GetOwinContext().GetAutofacLifetimeScope();
            var interviewRepository = autofacLifetimeScope
                .Resolve<IStatefulInterviewRepository>();

            IStatefulInterview interview = interviewRepository.Get(interviewId);

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
