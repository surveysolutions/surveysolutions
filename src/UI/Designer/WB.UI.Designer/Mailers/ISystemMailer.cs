using Mvc.Mailer;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Mailers
{
    public interface ISystemMailer
    {
        MvcMailMessage ConfirmationEmail(EmailConfirmationModel model);
        MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model);

        MvcMailMessage GetShareChangeNotificationEmail(SharingNotificationModel model);
        MvcMailMessage GetOwnerShareChangeNotificationEmail(SharingNotificationModel model);
    }
}