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
    }
}