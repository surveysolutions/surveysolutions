namespace WB.UI.Designer.Mailers
{
    using Mvc.Mailer;

    using WB.UI.Designer.Models;

    /// <summary>
    /// The SystemMailer interface.
    /// </summary>
    public interface ISystemMailer
    {
        #region Public Methods and Operators

        MvcMailMessage ConfirmationEmail(EmailConfirmationModel model);
        MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model);

        MvcMailMessage GetShareNotificationEmail(SharingNotificationModel model);
        MvcMailMessage GetStopShareNotificationEmail(SharingNotificationModel model);

        MvcMailMessage GetOwnerShareNotificationEmail(SharingNotificationModel model);
        MvcMailMessage GetOwnerStopShareNotificationEmail(SharingNotificationModel model);

        #endregion
    }
}