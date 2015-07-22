using Mvc.Mailer;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Mailers
{
    public interface ISystemMailer
    {
        MvcMailMessage ConfirmationEmail(EmailConfirmationModel model);
        MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model);

        MvcMailMessage GetShareNotificationEmail(SharingNotificationModel model);
        MvcMailMessage GetStopShareNotificationEmail(SharingNotificationModel model);

        MvcMailMessage GetOwnerShareNotificationEmail(SharingNotificationModel model);
        MvcMailMessage GetOwnerStopShareNotificationEmail(SharingNotificationModel model);
    }
}