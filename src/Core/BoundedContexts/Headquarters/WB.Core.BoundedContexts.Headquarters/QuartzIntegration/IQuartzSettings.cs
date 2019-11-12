using System;
using System.Collections.Specialized;
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
        private string instanceId;

        public QuartzSettings(UnitOfWorkConnectionSettings connectionSettings,
            string instanceId,
            bool isClustered)
        {
            this.connectionSettings = connectionSettings;
            this.instanceId = instanceId;
            this.IsClustered = isClustered;
        }

        public NameValueCollection GetSettings()
        {
            var isAutoMode = instanceId == "AUTO_MachineName";
            if (isAutoMode)
            {
                instanceId = Environment.MachineName;
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
                ["quartz.scheduler.instanceId"] = instanceId,
                ["quartz.jobStore.clustered"] = IsClustered.ToString()
            };

            return properties;
        }

        public bool IsClustered { get; }
    }
}
