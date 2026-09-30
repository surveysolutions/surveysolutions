using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier, IPendingNotificationsSender
    {
        private readonly IEmailSender mailer;
        private readonly IViewRenderService renderingService;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly ILogger<MailNotifier> logger;
        private readonly List<PendingNotification> pendingNotifications = new List<PendingNotification>();

        private record PendingNotification(string Email, Func<Task<string>> RenderMessage);

        public MailNotifier(IEmailSender mailer,
            IViewRenderService renderingService,
            IHttpContextAccessor contextAccessor,
            IUrlHelperFactory urlHelperFactory,
            ILogger<MailNotifier> logger)
        {
            this.logger = logger;
            this.mailer = mailer;
            this.renderingService = renderingService;
            this.contextAccessor = contextAccessor;
            this.urlHelperFactory = urlHelperFactory;
        }

        public void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType,
            string email,
            string? userName,
            string questionnaireId,
            string questionnaireTitle,
            ShareType shareType,
            string? actionPersonEmail)
        {
            if (contextAccessor.HttpContext == null)
                throw new Exception("Invalid context");
            
            IUrlHelper urlHelper = this.GetUrlHelper(contextAccessor.HttpContext);

            var sharingNotificationModel = new SharingNotificationModel
            {
                ShareChangeType = shareChangeType,
                Email = email.ToWBEmailAddress(),
                UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                QuestionnaireId = questionnaireId,
                QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) 
                    ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link 
                    : questionnaireTitle,
                ShareTypeName = shareType == ShareType.Edit 
                    ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit 
                    : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) 
                    ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user 
                    : actionPersonEmail,
                QuestionnaireLink = urlHelper.Action("Details", "Q", new { id = questionnaireId }, "https")
            };

            this.pendingNotifications.Add(new PendingNotification(email,
                () => this.GetShareChangeNotificationEmail(sharingNotificationModel)));
        }

        public void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string? actionPersonEmail, string sharedWithPersonEmail)
        {
            if (contextAccessor.HttpContext == null)
                throw new Exception("Invalid context");
            
            IUrlHelper urlHelper = this.GetUrlHelper(contextAccessor.HttpContext);
            var sharingNotificationModel = new SharingNotificationModel
            {
                ShareChangeType = shareChangeType,
                Email = email.ToWBEmailAddress(),
                UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                QuestionnaireId = questionnaireId,
                QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                ShareTypeName = shareType == ShareType.Edit 
                    ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit 
                    : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail,
                SharedWithPersonEmail = String.IsNullOrWhiteSpace(sharedWithPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : sharedWithPersonEmail,
                QuestionnaireLink = urlHelper.Action("Details", "Q", new { id = questionnaireId }, "https")
            };

            this.pendingNotifications.Add(new PendingNotification(email,
                () => this.GetOwnerShareChangeNotificationEmail(sharingNotificationModel)));
        }

        public async Task SendPendingNotificationsAsync()
        {
            var notifications = this.pendingNotifications.ToList();
            this.pendingNotifications.Clear();

            foreach (var notification in notifications)
            {
                try
                {
                    var message = await notification.RenderMessage();
                    await this.mailer.SendEmailAsync(notification.Email,
                        NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification,
                        message);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Failed to send questionnaire sharing notification");
                }
            }
        }

        public void DiscardPendingNotifications() => this.pendingNotifications.Clear();

        public async Task<string> GetShareChangeNotificationEmail(SharingNotificationModel model)
        {
            string? email = null;

            switch (model.ShareChangeType)
            {
                case ShareChangeType.Share: email = "Emails/TargetPersonShareNotification"; break;
                case ShareChangeType.StopShare: email = "Emails/TargetPersonStopShareNotification"; break;
                case ShareChangeType.TransferOwnership: email = "Emails/TranfserOwnershipNotification"; break;
            }
            ArgumentNullException.ThrowIfNull(email);

            var view = await this.renderingService.RenderToStringAsync(email, model);
            return view;
        }

        public async Task<string> GetOwnerShareChangeNotificationEmail(SharingNotificationModel model)
        {
            var view = await this.renderingService.RenderToStringAsync(
                model.ShareChangeType == ShareChangeType.Share
                    ? "Emails/OwnerShareNotification"
                    : "Emails/OwnerStopShareNotification", model);
            return view;
        }

        private IUrlHelper GetUrlHelper(HttpContext httpContext)
        {
            var actionContext = new ActionContext(
                httpContext,
                httpContext.GetRouteData(),
                new ActionDescriptor());

            return this.urlHelperFactory.GetUrlHelper(actionContext);
        }
    }
}
