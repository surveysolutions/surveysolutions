using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity.UI.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier
    {
        private readonly IEmailSender mailer;
        private readonly IViewRenderingService renderingService;

        public MailNotifier(IEmailSender mailer,
            IViewRenderingService renderingService)
        {
            this.mailer = mailer;
            this.renderingService = renderingService;
        }

        public void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType,
            string email,
            string userName,
            string questionnaireId,
            string questionnaireTitle,
            ShareType shareType,
            string actionPersonEmail)
        {
            //@Html.ActionLink(Model.QuestionnaireDisplayTitle, "Details", "Questionnaire", "https", null, null, new { id = Model.QuestionnaireId }, null)

            var sharingNotificationModel = new SharingNotificationModel
            {
                ShareChangeType = shareChangeType,
                Email = email.ToWBEmailAddress(),
                UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                QuestionnaireId = questionnaireId,
                QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                ShareTypeName = shareType == ShareType.Edit ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail
            };
            var message = this.GetShareChangeNotificationEmail(
                                sharingNotificationModel);
            message.ContinueWith(s =>
            {
                this.mailer.SendEmailAsync(email,
                    NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification,
                    message.Result);
            });
        }

        public void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail)
        {
            var sharingNotificationModel = new SharingNotificationModel
            {
                ShareChangeType = shareChangeType,
                Email = email.ToWBEmailAddress(),
                UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                QuestionnaireId = questionnaireId,
                QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                ShareTypeName = shareType == ShareType.Edit ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail,
                SharedWithPersonEmail = String.IsNullOrWhiteSpace(sharedWithPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : sharedWithPersonEmail

            };
            var message = this.GetOwnerShareChangeNotificationEmail(
                                sharingNotificationModel);

            message.ContinueWith((state) =>
            {
                this.mailer.SendEmailAsync(email,
                    NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification,
                    message.Result);
            });
        }

        public async Task<string> GetShareChangeNotificationEmail(SharingNotificationModel model)
        {
            var view = await this.renderingService.RenderToStringAsync(
                model.ShareChangeType == ShareChangeType.Share
                    ? "TargetPersonShareNotification"
                    : "TargetPersonStopShareNotification",
                model);
            return view;
        }

        public async Task<string> GetOwnerShareChangeNotificationEmail(SharingNotificationModel model)
        {
            var view = await this.renderingService.RenderToStringAsync(
                model.ShareChangeType == ShareChangeType.Share
                    ? "OwnerShareNotification"
                    : "OwnerStopShareNotification", model);
            return view;
        }
    }
}
