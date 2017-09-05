using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
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

            hub.Clients.OthersInGroup(interviewId).closeInterview();

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
    }
}