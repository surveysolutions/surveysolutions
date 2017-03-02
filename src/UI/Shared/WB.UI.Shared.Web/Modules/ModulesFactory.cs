using System.Web.Configuration;
using Ninject.Modules;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;

namespace WB.UI.Shared.Web.Modules
{
    public class ModulesFactory
    {
        public static NinjectModule GetEventStoreModule()
        {
            var eventStoreSettings = new PostgreConnectionSettings();
            eventStoreSettings.ConnectionString = WebConfigurationManager.ConnectionStrings["Postgres"].ConnectionString;
            eventStoreSettings.SchemaName = "events";

            return new PostgresWriteSideModule(eventStoreSettings,
                new DbUpgradeSettings(typeof(M001_AddEventSequenceIndex).Assembly, typeof(M001_AddEventSequenceIndex).Namespace));
        }
    }
}