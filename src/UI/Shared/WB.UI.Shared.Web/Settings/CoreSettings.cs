using System;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Settings
{
    public class CoreSettings
    {
        private static IConfigurationManager config
        {
            get { return ServiceLocator.Current.GetInstance<IConfigurationManager>(); }
        }

        public static bool IsUnderDevelopment
        {
            get { return bool.Parse(config.AppSettings["IsUnderDevelopment"]); }
        }
    }
}