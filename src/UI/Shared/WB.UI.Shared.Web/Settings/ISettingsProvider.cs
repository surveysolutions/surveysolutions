using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace WB.UI.Shared.Web.Settings
{
    public interface ISettingsProvider
    {
        IEnumerable<ApplicationSetting> GetSettings();

        TSection GetSection<TSection>(string name) where TSection : ConfigurationSection;
        NameValueCollection AppSettings { get; }
        ConnectionStringSettingsCollection ConnectionStrings { get; }
    }
}