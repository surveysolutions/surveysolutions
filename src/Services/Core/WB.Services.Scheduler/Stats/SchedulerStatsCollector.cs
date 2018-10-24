using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.Advanced;

namespace WB.Services.Scheduler.Stats
{
    public class SchedulerStatsCollector : IOnDemandCollector
    {
        private readonly IServiceProvider serviceProvider;

        private static readonly Gauge CurrentJobs = Metrics.CreateGauge(
            "wb_services_scheduler_jobs_count",
            "Count of current jobs states per type", "type", "status");

        public SchedulerStatsCollector(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void RegisterMetrics(ICollectorRegistry registry)
        {
            registry.GetOrAdd(CurrentJobs);
        }

        public void UpdateMetrics()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<JobContext>();
                /*  select "type", status, count(*)
                    from scheduler.jobs
                    group by "type", status  */

                var statusCounts = from job in db.Jobs
                    group job by new {job.Type, job.Status, job.TenantName}
                    into g
                    select ValueTuple.Create(g.Key, g.Count());

                foreach (var status in statusCounts)
                {
                    CurrentJobs.Labels(status.Item1.Type, status.Item1.Status.ToString(), status.Item1.TenantName)
                        .Set(status.Item2);
                }
            }
        }
    }
}
