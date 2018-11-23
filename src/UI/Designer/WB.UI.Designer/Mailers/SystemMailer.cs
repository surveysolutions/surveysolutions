using Mvc.Mailer;
using WB.Core.BoundedContexts.Designer.Views;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Mailers
{
    public class SystemMailer : MailerBase, ISystemMailer
    {
        public SystemMailer()
        {
            this.MasterName = "_Layout";
        }

        public virtual MvcMailMessage ConfirmationEmail(EmailConfirmationModel model)
        {
            return this.GetMessage(model, NotificationResources.SystemMailer_ConfirmationEmail_Complete_Registration_Process, "ConfirmationEmail");
        }

        public virtual MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model)
        {
            return this.GetMessage(model, NotificationResources.SystemMailer_ResetPasswordEmail_Complete_Password_Reset, "ResetPasswordEmail");
        }

        public MvcMailMessage GetShareChangeNotificationEmail(SharingNotificationModel model)
        {
            return this.GetMessage(model, NotificationResources.SystemMailer_GetShareNotificationEmail_Questionnaire_sharing_notification, model.ShareChangeType == ShareChangeType.Share ? "TargetPersonShareNotification" : "TargetPersonStopShareNotification");
        }

        public MvcMailMessage GetOwnerShareChangeNotificationEmail(SharingNotificationModel model)
        {
            return this.GetMessage(model, NotificationResources.SystemMailer_GetOwnerShareNotificationEmail_Your_questionnaire_sharing_notification, model.ShareChangeType == ShareChangeType.Share ? "OwnerShareNotification" : "OwnerStopShareNotification");
        }

        private MvcMailMessage GetMessage(IEmailNotification model, string subject, string viewName)
        {
            this.ViewData.Model = model;
            var message = this.Populate(
                x =>
                {
                    x.Subject = subject;
                    x.ViewName = viewName;
                    x.To.Add(model.Email);
                });

            return message;
        }
    }
}
