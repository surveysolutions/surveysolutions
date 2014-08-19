using System;
using System.Web.Configuration;

namespace WB.UI.Shared.Web.Settings
{
    public static class CoreSettings
    {
        public static StoreProviders EventStoreProvider
        {
            get { return (StoreProviders) Enum.Parse(typeof(StoreProviders), WebConfigurationManager.AppSettings["Core.EventStoreProvider"], true); }
        } 
    }
}