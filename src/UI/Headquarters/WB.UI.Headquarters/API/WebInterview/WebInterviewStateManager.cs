using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Clients.CallerState.interviewId as string;
            var sectionId = context.Hub.Clients.CallerState.sectionId as string;

            if (interviewId != null)
            {
                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, WebInterview.GetConnectedClientSectionKey(sectionId, interviewId));
                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, interviewId);
            }

            return base.OnAfterIncoming(result, context);
        }
    }
}