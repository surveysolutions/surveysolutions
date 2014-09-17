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
            var storeProvider =
                (StoreProviders)
                    Enum.Parse(typeof (StoreProviders), WebConfigurationManager.AppSettings["Core.EventStoreProvider"], true);

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
                bool isEmbeded;
                if (!bool.TryParse(WebConfigurationManager.AppSettings["Raven.IsEmbeded"], out isEmbeded))
                {
                    isEmbeded = false;
                }

                string storePath = isEmbeded
                    ? WebConfigurationManager.AppSettings["Raven.DocumentStoreEmbeded"]
                    : WebConfigurationManager.AppSettings["Raven.DocumentStore"];

                var ravenSettings = new RavenConnectionSettings(storePath, isEmbeded, WebConfigurationManager.AppSettings["Raven.Username"],
                    WebConfigurationManager.AppSettings["Raven.Password"], WebConfigurationManager.AppSettings["Raven.Databases.Events"],
                    WebConfigurationManager.AppSettings["Raven.Databases.Views"],
                    WebConfigurationManager.AppSettings["Raven.Databases.PlainStorage"],
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