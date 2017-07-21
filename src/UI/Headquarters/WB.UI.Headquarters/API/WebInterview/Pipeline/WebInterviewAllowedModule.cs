using System;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewAllowedModule : HubPipelineModule
    {
        private IWebInterviewNotificationService webInterviewNotificationService =>
            ServiceLocator.Current.GetInstance<IWebInterviewNotificationService>();

        private IWebInterviewAllowService webInterviewAllowService =>
            ServiceLocator.Current.GetInstance<IWebInterviewAllowService>();

        protected override bool OnBeforeConnect(IHub hub)
        {
            this.CheckWebInterviewAccessPermissions(hub);

            return base.OnBeforeConnect(hub);
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var hub = context.Hub;

            try
            {
                this.CheckWebInterviewAccessPermissions(hub);
            }
            catch (WebInterviewAccessException)
            {
                var interviewId = hub.Context.QueryString.Get(@"interviewId");
                if (!interviewId.IsNullOrWhiteSpace())
                {
                    webInterviewNotificationService.ReloadInterview(Guid.Parse(interviewId));
                }
            }

            return base.OnBeforeIncoming(context);
        }

        private void CheckWebInterviewAccessPermissions(IHub hub)
        {
            webInterviewAllowService.CheckWebInterviewAccessPermissions(hub.Context.QueryString.Get(@"interviewId"));
        }
    }
}