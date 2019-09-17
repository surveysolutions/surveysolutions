using System;
using System.Collections.Specialized;
using System.Configuration;
using WB.Core.GenericSubdomains.Portable;
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
            var instanceid = ConfigurationManager.AppSettings["Scheduler.InstanceId"];
            var isAutoMode = instanceid == "AUTO_MachineName";
            if (isAutoMode)
            {
                instanceid = Environment.MachineName;
            }

            if (!IsClustered)
            {
                return new NameValueCollection
                {
                    ["quartz.jobStore.type"] = "Quartz.Simpl.RAMJobStore, Quartz"
                };
            }

            var properties = new NameValueCollection
            {
                ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz",
                ["quartz.jobStore.dataSource"] = "default",
                ["quartz.dataSource.default.connectionString"] = connectionSettings.ConnectionString,
                ["quartz.dataSource.default.provider"] = "Npgsql-30",
                ["quartz.jobStore.tablePrefix"] = "quartz.",
                ["quartz.serializer.type"] = "binary",
                ["quartz.scheduler.instanceId"] = instanceid,
                ["quartz.jobStore.clustered"] = IsClustered.ToString()
            };

            return properties;
        }

        public bool IsClustered => ConfigurationManager.AppSettings["Scheduler.IsClustered"].ToBool(false);
    }
}
