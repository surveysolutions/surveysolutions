using Mvc.Mailer;
using WB.UI.Designer.Models;

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
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                    {
                        x.Subject = "Complete Registration Process";
                        x.ViewName = "ConfirmationEmail";
                        x.To.Add(model.Email);
                    });
        }

        public virtual MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                    {
                        x.Subject = "Complete Password Reset";
                        x.ViewName = "ResetPasswordEmail";
                        x.To.Add(model.Email);
                    });
        }
        public MvcMailMessage GetShareNotificationEmail(SharingNotificationModel model)
        {
            this.ViewData.Model = model;
            var message = this.Populate(
                x =>
                {
                    x.Subject = "Questionnaire sharing notification";
                    x.ViewName = "TargetPersonShareNotification";
                    x.To.Add(model.Email);
                });

            return message;
        }

        public MvcMailMessage GetStopShareNotificationEmail(SharingNotificationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                {
                    x.Subject = "Questionnaire stop sharing notification";
                    x.ViewName = "TargetPersonStopShareNotification";
                    x.To.Add(model.Email);
                });
        }

        public MvcMailMessage GetOwnerShareNotificationEmail(SharingNotificationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                {
                    x.Subject = "Your questionnaire sharing notification";
                    x.ViewName = "OwnerShareNotification";
                    x.To.Add(model.Email);
                });
        }

        public MvcMailMessage GetOwnerStopShareNotificationEmail(SharingNotificationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                {
                    x.Subject = "Your questionnaire stop sharing notification";
                    x.ViewName = "OwnerStopShareNotification";
                    x.To.Add(model.Email);
                });
        }

    }
}