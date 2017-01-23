using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewStateManager : HubPipelineModule
    {
        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Clients.CallerState.interviewId;
            var sectionId = context.Hub.Clients.CallerState.sectionId;

            if (interviewId != null)
            {
                context.Hub.Groups.Add(context.Hub.Context.ConnectionId, $"{sectionId?.ToString() ?? ""}_{interviewId}");
            }

            return base.OnAfterIncoming(result, context);
        }
    }
}