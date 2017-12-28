using System;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class InterviewAuthorizationModule : HubPipelineModule
    {
        private IWebInterviewNotificationService webInterviewNotificationService =>
            ServiceLocator.Current.GetInstance<IWebInterviewNotificationService>();

        private IWebInterviewAllowService webInterviewAllowService =>
            ServiceLocator.Current.GetInstance<IWebInterviewAllowService>();

        private IReviewAllowedService reviewAllowedService =>
            ServiceLocator.Current.GetInstance<IReviewAllowedService>();


        protected override bool OnBeforeConnect(IHub hub)
        {
            this.CheckPermissions(hub);

            return base.OnBeforeConnect(hub);
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var hub = context.Hub;

            try
            {
                CheckPermissions(hub);
            }
            catch (InterviewAccessException)
            {
                var interviewId = hub.Context.QueryString.Get(@"interviewId");
                if (!interviewId.IsNullOrWhiteSpace())
                {
                    webInterviewNotificationService.ReloadInterview(Guid.Parse(interviewId));
                }
            }

            return base.OnBeforeIncoming(context);
        }

        private void CheckPermissions(IHub hub)
        {
            var isReview = hub.Context.QueryString[@"review"].ToBool(false);
            var interviewId = hub.Context.QueryString.Get(@"interviewId");
            if (!isReview)
            {
                this.webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId);
            }
            else
            {
                this.reviewAllowedService.CheckIfAllowed(Guid.Parse(interviewId));
            }
        }
    }
}