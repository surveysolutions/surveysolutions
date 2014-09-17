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

        public static StoreProviders EventStoreProvider
        {
            get { return (StoreProviders)Enum.Parse(typeof(StoreProviders), config.AppSettings["Core.EventStoreProvider"], true); }
        }

        public static bool IsUnderDevelopment
        {
            get { return bool.Parse(config.AppSettings["IsUnderDevelopment"]); }
        }
    }
}