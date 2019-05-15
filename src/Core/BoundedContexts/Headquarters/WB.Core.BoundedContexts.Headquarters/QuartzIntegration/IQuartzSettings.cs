using System.Collections.Specialized;
using System.Configuration;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public interface IQuartzSettings
    {
        NameValueCollection GetSettings();
    }

    class QuartzSettings : IQuartzSettings
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;

        public QuartzSettings(UnitOfWorkConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        public NameValueCollection GetSettings()
        {
            var properties = new NameValueCollection
            {
                ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz",
                ["quartz.jobStore.dataSource"] = "default",
                ["quartz.dataSource.default.connectionString"] = connectionSettings.ConnectionString,
                ["quartz.dataSource.default.provider"] = "Npgsql-30",
                ["quartz.jobStore.tablePrefix"] = "quartz.",
                ["quartz.serializer.type"] = "binary",
                ["quartz.scheduler.instanceId"] = "AUTO",
                ["quartz.jobStore.clustered"] = ConfigurationManager.AppSettings["Scheduler.IsClustered"]
            };

            return properties;
        }
    }
}
