using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Hub
{
    public class DesignerTokenRenewalPipeline : HubPipelineModule
    {
        private readonly IDesignerWebTesterApi webTesterApi;

        public DesignerTokenRenewalPipeline(IDesignerWebTesterApi webTesterApi)
        {
            this.webTesterApi = webTesterApi;
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var interviewId = context.Hub.Context.QueryString[@"interviewId"];
            var questionnaire = webTesterApi.GetQuestionnaireInfoAsync(Guid.Parse(interviewId).ToString());
            //TODO: add notification to client here if questionnaire were updated
            
            return base.OnBeforeIncoming(context);
        }
    }
}