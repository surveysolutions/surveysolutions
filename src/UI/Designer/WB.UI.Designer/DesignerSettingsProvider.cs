using System.Collections.Generic;
using System.Net.Configuration;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Designer
{
    public class DesignerSettingsProvider : SettingsProvider
    {
        protected override List<string> settingsToSkip => new List<string>(base.settingsToSkip)
        {
            "ReCaptchaPrivateKey"
        };

        public override IEnumerable<ApplicationSetting> GetSettings()
        {
            foreach (var applicationSetting in base.GetSettings())
            {
                yield return applicationSetting;
            }

            var smtpSection = GetSection<SmtpSection>("system.net/mailSettings/smtp");

            yield return new ApplicationSetting("MailSettings.From", smtpSection.From);
            yield return new ApplicationSetting("MailSettings.DeliveryMethod", smtpSection.DeliveryMethod);
            yield return new ApplicationSetting("MailSettings.Network.DefaultCredentials", smtpSection.Network.DefaultCredentials);
            yield return new ApplicationSetting("MailSettings.Network.Host", smtpSection.Network.Host);
            yield return new ApplicationSetting("MailSettings.Network.Port", smtpSection.Network.Port);
            yield return new ApplicationSetting("MailSettings.Network.EnableSsl", smtpSection.Network.EnableSsl);
            yield return new ApplicationSetting("MailSettings.Network.UserName", smtpSection.Network.UserName);
        }
    }
}
