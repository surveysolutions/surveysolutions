using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier
    {
        private readonly IEmailSender mailer;
        private readonly IViewRenderService renderingService;
        private readonly IActionContextAccessor contextAccessor;
        private readonly IUrlHelperFactory urlHelperFactory;

        public MailNotifier(IEmailSender mailer,
            IViewRenderService renderingService,
            IActionContextAccessor contextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
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
            if (contextAccessor?.ActionContext == null)
                throw new Exception("Invalid context");
            
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext);

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
                QuestionnaireLink = urlHelper.Action("Details", "Questionnaire", new { id = questionnaireId }, "https")
            };

            var message = this.GetShareChangeNotificationEmail(sharingNotificationModel);

            message.ContinueWith(s =>
            {
                this.mailer.SendEmailAsync(email,
                    NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification,
                    message.Result);
            });
        }

        public void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string? actionPersonEmail, string sharedWithPersonEmail)
        {
            if (contextAccessor?.ActionContext == null)
                throw new Exception("Invalid context");
            
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext);
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
                QuestionnaireLink = urlHelper.Action("Details", "Questionnaire", new { id = questionnaireId }, "https")

            };
            var message = this.GetOwnerShareChangeNotificationEmail(sharingNotificationModel);

            message.ContinueWith((state) =>
            {
                this.mailer.SendEmailAsync(email,
                    NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification,
                    message.Result);
            });
        }

        public async Task<string> GetShareChangeNotificationEmail(SharingNotificationModel model)
        {
            string? email = null;

            switch (model.ShareChangeType)
            {
                case ShareChangeType.Share: email = "Emails/TargetPersonShareNotification"; break;
                case ShareChangeType.StopShare: email = "Emails/TargetPersonStopShareNotification"; break;
                case ShareChangeType.TransferOwnership: email = "Emails/TranfserOwnershipNotification"; break;
            }

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
    }
}
