using System;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier
    {
        public MailNotifier(ISystemMailer mailer)
        {
            this.mailer = mailer;
        }

        private readonly ISystemMailer mailer;
        
        public void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType, string email, string userName, Guid questionnaireId, 
            string questionnaireTitle, ShareType shareType, string actionPersonEmail)
        {
            var message = this.mailer.GetShareChangeNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    ShareChangeType = shareChangeType,
                                    Email = email.ToWBEmailAddress(),
                                    UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                                    QuestionnaireId = questionnaireId,
                                    QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                                    ShareTypeName = shareType == ShareType.Edit ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationMessages.MailNotifier_NotifyOwnerAboutShareChange_view,
                                    ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail
                                });
            message.SendAsync();
        }
        
        public void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, Guid questionnaireId, string questionnaireTitle,
            ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail)
        {
            var message = this.mailer.GetOwnerShareChangeNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    ShareChangeType = shareChangeType,
                                    Email = email.ToWBEmailAddress(),
                                    UserCallName = String.IsNullOrWhiteSpace(userName) ? email : userName,
                                    QuestionnaireId = questionnaireId,
                                    QuestionnaireDisplayTitle = String.IsNullOrWhiteSpace(questionnaireTitle) ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_link : questionnaireTitle,
                                    ShareTypeName = shareType == ShareType.Edit ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_edit : NotificationMessages.MailNotifier_NotifyOwnerAboutShareChange_view,
                                    ActionPersonCallName = String.IsNullOrWhiteSpace(actionPersonEmail) ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_user : actionPersonEmail,
                                    SharedWithPersonEmail = String.IsNullOrWhiteSpace(sharedWithPersonEmail) ? NotificationMessages.MailNotifier_NotifyTargetPersonAboutShareChange_user : sharedWithPersonEmail

                                });
            message.SendAsync();
        }
        
    }
}
