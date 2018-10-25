using System;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.Advanced;
using WB.Services.Scheduler.Model;

namespace WB.Services.Scheduler.Stats
{
    public class SchedulerStatsCollector : IOnDemandCollector
    {
        private readonly IServiceProvider serviceProvider;

        private static readonly Gauge CurrentJobs = Metrics.CreateGauge(
            "wb_services_scheduler_jobs_count",
            "Count of current jobs states per type", "type", "status", "tenant");

        public SchedulerStatsCollector(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void RegisterMetrics(ICollectorRegistry registry)
        {
            registry.GetOrAdd(CurrentJobs);
        }

        private readonly string JobStatuses = 
            string.Join(",",
            Enum.GetValues(typeof(JobStatus))
                .OfType<JobStatus>()
                .Select(js => $"'{js.ToString().ToLower()}'"));


        public void UpdateMetrics()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();
                
                var query = @"
            with
                tenants as (select distinct tenant_name as tenant from scheduler.jobs),
                statuses as (select s as status from unnest(ARRAY[" + JobStatuses + @"]) s),
                types as (select distinct ""type"" from scheduler.jobs),
                tuples as (select* from tenants t, statuses s, types tp)
            select t.tenant, t.status, t.""type"", 
            (
                select count(*) from scheduler.jobs j
                where j.tenant_name = t.tenant and j.""type"" = t.""type"" and j.status = t.status
                ) as ""count""
            from tuples t";

                var counts = db.Database.GetDbConnection().Query<(string tenant, string status, string type, int count)>(query);

                foreach (var count in counts)
                {
                    CurrentJobs.Labels(count.type, count.status, count.tenant).Set(count.count);
                }
            }
        }
    }
}
