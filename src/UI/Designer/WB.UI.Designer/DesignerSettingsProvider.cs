using System.Collections.Generic;
using System.Net.Configuration;
using System.Web.Configuration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
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

            var dynamicCompilerSettings = (IDynamicCompilerSettingsGroup)WebConfigurationManager.GetSection("dynamicCompilerSettingsGroup");

            //int esentCacheSize = ;
            //var hqSettings = (HeadquartersSettings)WebConfigurationManager.GetSection("dynamicCompilerSettingsGroup/settings");

            foreach (var settings in  dynamicCompilerSettings.SettingsCollection)
            {
                yield return new ApplicationSetting(string.Format("DynamicCompilerSettings.{0}.Name", settings.Name), settings.Name);
                yield return new ApplicationSetting(string.Format("DynamicCompilerSettings.{0}.PortableAssembliesPath", settings.Name), settings.PortableAssembliesPath);
                yield return new ApplicationSetting(string.Format("DynamicCompilerSettings.{0}.PortableAssemblies", settings.Name), 
                    string.Join(";",settings.DefaultReferencedPortableAssemblies));
            }

            var smtpSection = (SmtpSection)WebConfigurationManager.GetSection("system.net/mailSettings/smtp");
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
