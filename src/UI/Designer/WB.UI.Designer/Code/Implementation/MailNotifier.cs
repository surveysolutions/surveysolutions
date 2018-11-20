using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier
    {
        public MailNotifier(ILogger logger, ISystemMailer mailer)
        {
            this.logger = logger;
            this.mailer = mailer;
        }

        private readonly ILogger logger;
        private readonly ISystemMailer mailer;

        public void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail)
        {
            var message = this.mailer.GetShareChangeNotificationEmail(
                                new SharingNotificationModel
                                {
                                    ShareChangeType = shareChangeType,
                                    Email = email.ToWBEmailAddress(),
                                    UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                                    QuestionnaireId = questionnaireId,
                                    QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                                    ShareTypeName = shareType == ShareType.Edit ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                                    ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail
                                });
            message.SendAsync().ContinueWith(exception => logger.Error("Sending failed: " + exception), TaskContinuationOptions.OnlyOnFaulted);
        }
        
        public void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail)
        {
            var message = this.mailer.GetOwnerShareChangeNotificationEmail(
                                new SharingNotificationModel
                                {
                                    ShareChangeType = shareChangeType,
                                    Email = email.ToWBEmailAddress(),
                                    UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                                    QuestionnaireId = questionnaireId,
                                    QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                                    ShareTypeName = shareType == ShareType.Edit ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationResources.MailNotifier_NotifyOwnerAboutShareChange_view,
                                    ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail,
                                    SharedWithPersonEmail = String.IsNullOrWhiteSpace(sharedWithPersonEmail) ? NotificationResources.MailNotifier_NotifyTargetPersonAboutShareChange_user : sharedWithPersonEmail

                                });

            message.SendAsync().ContinueWith(exception => logger.Error("Sending failed: " + exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
