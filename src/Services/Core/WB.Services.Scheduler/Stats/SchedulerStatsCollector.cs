#nullable enable
using System;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace WB.Services.Scheduler.Stats
{
    public static class SchedulerMetricsExtensions
    {
        public static void UseSchedulerMetrics(this IApplicationBuilder app, CollectorRegistry? registry = null)
        {
            var collector = new SchedulerStatsCollector(app.ApplicationServices);
            collector.Register(registry);
        }
    }
    
    internal class SchedulerStatsCollector
    {
        private readonly IServiceProvider serviceProvider;

        private static readonly Gauge CurrentJobs = Metrics.CreateGauge(
            "wb_services_scheduler_jobs_count",
            "Count of current jobs states per type", "type", "status", "tenant");

        public SchedulerStatsCollector(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Register(CollectorRegistry? registry = null)
        {
            (registry ?? Metrics.DefaultRegistry).AddBeforeCollectCallback(UpdateMetrics);
        }

        public void UpdateMetrics()
        {
            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<JobContext>();
                
            var query = @"select j.tenant_name as ""tenant"", j.status, j.""type"", count(*) as ""count""
                from scheduler.jobs j
                group by 1,2,3";

            var counts = db.Database.GetDbConnection().Query<(string tenant, string status, string type, int count)>(query);

            foreach (var count in counts)
            {
                CurrentJobs.Labels(count.type, count.status, count.tenant).Set(count.count);
            }
        }
    }
}
