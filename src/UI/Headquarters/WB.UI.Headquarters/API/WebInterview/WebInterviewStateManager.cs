using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            context.Hub.Groups.Add(context.Hub.Context.ConnectionId, context.Hub.Clients.CallerState.interviewId);
            return base.OnAfterIncoming(result, context);
        }
    }
}