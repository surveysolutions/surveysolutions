using System;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly IWebInterviewNotificationService webInterviewNotificationService;

        private readonly IWebInterviewAllowService webInterviewAllowService;

        private readonly IReviewAllowedService reviewAllowedService;

        private readonly IAuthorizedUser authorizedUser;

        public WebInterviewAuthorizeAttribute() : 
            this(

                //resolve from context to preserve scope
                //create new scope instead
            ServiceLocator.Current.GetInstance<IWebInterviewNotificationService>(),
            ServiceLocator.Current.GetInstance<IWebInterviewAllowService>(),
            ServiceLocator.Current.GetInstance<IReviewAllowedService>(),
            ServiceLocator.Current.GetInstance<IAuthorizedUser>()
            )
        {
        }

        public WebInterviewAuthorizeAttribute(
            IWebInterviewNotificationService webInterviewNotificationService,
            IWebInterviewAllowService webInterviewAllowService,
            IReviewAllowedService reviewAllowedService, 
            IAuthorizedUser authorizedUser)
        {
            this.webInterviewNotificationService = webInterviewNotificationService ?? throw new ArgumentNullException(nameof(webInterviewNotificationService));
            this.webInterviewAllowService = webInterviewAllowService ?? throw new ArgumentNullException(nameof(webInterviewAllowService));
            this.reviewAllowedService = reviewAllowedService ?? throw new ArgumentNullException(nameof(reviewAllowedService));
            this.authorizedUser = authorizedUser ?? throw new ArgumentNullException(nameof(authorizedUser));
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            CheckPermissions(request);
            return true;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, 
            bool appliesToMethod)
        {
            try
            {
                if (hubIncomingInvokerContext.MethodDescriptor.Attributes.Any(x => x.TypeId.ToString() == ObserverNotAllowedAttribute.Id))
                {
                    if (this.authorizedUser.IsObserving) return false;
                }

                CheckPermissions(hubIncomingInvokerContext.Hub.Context.Request);

                return true;
            }
            catch (InterviewAccessException)
            {
                var interviewId = hubIncomingInvokerContext.Hub.Context.QueryString.Get(@"interviewId");
                if (!interviewId.IsNullOrWhiteSpace())
                {
                    webInterviewNotificationService.ReloadInterview(Guid.Parse(interviewId));
                }
            }

            var authorizeHubMethodInvocation = base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
            return authorizeHubMethodInvocation;
        }

        private void CheckPermissions(IRequest hub)
        {
            var isReview = hub.QueryString[@"review"].ToBool(false);
            var interviewId = hub.QueryString.Get(@"interviewId");

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
