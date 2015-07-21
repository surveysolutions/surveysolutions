using System;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code.Implementation
{
    public class MailNotifier : IRecipientNotifier
    {
        public MailNotifier(ISystemMailer mailer, ILogger logger)
        {
            this.mailer = mailer;
            this.logger = logger;
        }

        private readonly ISystemMailer mailer;
        private readonly ILogger logger;

        public void NotifyTargetPersonAboutShare(string email, string userName, Guid questionnaireId, 
            string questionnaireTitle, ShareType shareType, string actionPersonEmail)
        {
            var message = this.mailer.GetShareNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    Email = email.ToWBEmailAddress(),
                                    UserName = userName,
                                    QiestionnaireId = questionnaireId,
                                    QiestionnaireTitle = questionnaireTitle,
                                    ShareType = shareType,
                                    ActionPersonEmail = actionPersonEmail
                                });
            message.SendAsync();
        }

        public void NotifyTargetPersonAboutStopShare(string email, string userName, string questionnaireTitle, 
            string actionPersonEmail)
        {
            var message = this.mailer.GetStopShareNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    Email = email.ToWBEmailAddress(),
                                    UserName = userName,
                                    QiestionnaireTitle = questionnaireTitle,
                                    ActionPersonEmail = actionPersonEmail
                                });
            message.SendAsync();
        }

        public void NotifyOwnerAboutShare(string email, string userName, Guid questionnaireId, string questionnaireTitle,
            ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail)
        {
            var message = this.mailer.GetOwnerShareNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    Email = email.ToWBEmailAddress(),
                                    UserName = userName,
                                    QiestionnaireId = questionnaireId,
                                    QiestionnaireTitle = questionnaireTitle,
                                    ShareType = shareType,
                                    ActionPersonEmail = actionPersonEmail,
                                    SharedWithPersonEmail = sharedWithPersonEmail

                                });
            message.SendAsync();
        }

        public void NotifyOwnerAboutStopShare(string email, string userName, string questionnaireTitle, 
            string actionPersonEmail, string sharedWithPersonEmail)
        {
            var message = this.mailer.GetOwnerStopShareNotificationEmail(
                                new SharingNotificationModel()
                                {
                                    Email = email.ToWBEmailAddress(),
                                    UserName = userName,
                                    QiestionnaireTitle = questionnaireTitle,
                                    ActionPersonEmail = actionPersonEmail,
                                    SharedWithPersonEmail = sharedWithPersonEmail
                                });
            message.SendAsync();
        }
    }
}
