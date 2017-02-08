using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Context.QueryString[@"interviewId"];
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