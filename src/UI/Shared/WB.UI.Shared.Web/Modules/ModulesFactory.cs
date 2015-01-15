using System;
using System.Web.Configuration;
using Ninject.Modules;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.Raven;
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
                var eventStoreConnectionSettings = new EventStoreConnectionSettings();
                eventStoreConnectionSettings.ServerIP = WebConfigurationManager.AppSettings["EventStore.ServerIP"];
                eventStoreConnectionSettings.ServerTcpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["EventStore.ServerTcpPort"]);
                eventStoreConnectionSettings.ServerHttpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["EventStore.ServerHttpPort"]);
                eventStoreConnectionSettings.Login = WebConfigurationManager.AppSettings["EventStore.Login"];
                eventStoreConnectionSettings.Password = WebConfigurationManager.AppSettings["EventStore.Password"];

                return new EventStoreWriteSideModule(eventStoreConnectionSettings);
            }

            if (storeProvider == StoreProviders.Raven)
            {
                string storePath = WebConfigurationManager.AppSettings["Raven.DocumentStore"];

                var ravenSettings = new RavenConnectionSettings(
                    storagePath: storePath,
                    username: WebConfigurationManager.AppSettings["Raven.Username"],
                    password: WebConfigurationManager.AppSettings["Raven.Password"],
                    viewsDatabase: WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                    plainDatabase: WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
                    failoverBehavior: WebConfigurationManager.AppSettings["Raven.Databases.FailoverBehavior"],
                    activeBundles: WebConfigurationManager.AppSettings["Raven.Databases.ActiveBundles"]);

                bool useStreamingForAllEvents;
                if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.UseStreamingForAllEvents"], out useStreamingForAllEvents))
                {
                    useStreamingForAllEvents = true;
                }
                int? pageSize = GetEventStorePageSize();
                return pageSize.HasValue
                    ? new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents, pageSize.Value)
                    : new RavenWriteSideInfrastructureModule(ravenSettings, useStreamingForAllEvents);
            }

            return null;
        }

        private static int? GetEventStorePageSize()
        {
            int pageSize;

            if (int.TryParse(WebConfigurationManager.AppSettings["EventStorePageSize"], out pageSize))
                return pageSize;
            return null;
        }
    }
}