using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        private readonly IProductVersion productVersion;
        private readonly IServiceLocator serviceLocator;

        public WebInterviewStateManager(IProductVersion productVersion, IServiceLocator serviceLocator)
        {
            this.productVersion = productVersion;
            this.serviceLocator = serviceLocator;
        }

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.ReloadIfOlderVersion(hub);
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterConnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString[@"interviewId"];
            var interview = this.serviceLocator.GetInstance<IStatefulInterviewRepository>().Get(interviewId);

            hub.Clients.OthersInGroup(interviewId).closeInterview();

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
    }
}