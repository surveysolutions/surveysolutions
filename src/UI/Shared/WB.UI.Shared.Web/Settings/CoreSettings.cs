using System;
using Microsoft.Practices.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

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

        public static bool IsDevelopmentEnvironment
        {
            get { return config.AppSettings["IsDevelopmentEnvironment"].ToBool(false); }
        }

        public static bool IsHttpsRequired
        {
            get { return config.AppSettings["IsHttpsRequired"].ToBool(true); }
        }
    }
}