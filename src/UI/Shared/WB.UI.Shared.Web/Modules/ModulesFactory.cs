using System;
using System.Web.Configuration;
using Ninject.Modules;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Modules
{
    public class ModulesFactory
    {
        public static NinjectModule GetEventStoreModule()
        {
            var storeProvider = CoreSettings.EventStoreProvider;

            if (storeProvider == StoreProviders.EventStore)
            {
                var eventStoreConnectionSettings = new EventStoreSettings();
                
                eventStoreConnectionSettings.ServerIP = WebConfigurationManager.AppSettings["EventStore.ServerIP"];
                eventStoreConnectionSettings.ServerTcpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["EventStore.ServerTcpPort"]);
                eventStoreConnectionSettings.ServerHttpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["EventStore.ServerHttpPort"]);
                eventStoreConnectionSettings.Login = WebConfigurationManager.AppSettings["EventStore.Login"];
                eventStoreConnectionSettings.Password = WebConfigurationManager.AppSettings["EventStore.Password"];
                eventStoreConnectionSettings.MaxCountToRead = int.Parse(WebConfigurationManager.AppSettings["EventStore.MaxCountToRead"]);

                return new EventStoreWriteSideModule(eventStoreConnectionSettings);
            }
            else if (storeProvider == StoreProviders.Postgres)
            {
                var eventStoreSettings = new PostgreConnectionSettings();
                eventStoreSettings.ConnectionString = WebConfigurationManager.ConnectionStrings["Postgres"].ConnectionString;
                eventStoreSettings.SchemaName = "events";
                return new PostgresWriteSideModule(eventStoreSettings);
            }

            return null;
        }

        private static bool? TryParseBool(string value)
        {
            bool result;
            if (bool.TryParse(value, out result))
            {
                return result;
            }
            return false;
        }
    }
}