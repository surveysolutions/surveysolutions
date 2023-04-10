using Humanizer;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Metrics;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Workspaces;

namespace WB.UI.Headquarters.Metrics
{
    public class DashboardStatisticsService : BackgroundService, IDashboardStatisticsService
    {
        private readonly IWorkspacesCache workspaces;
        private readonly MemoryCache memoryCache;
        private ServerStatusResponse state;
        private DateTime lastQuery = DateTime.UtcNow;
        private readonly ManualResetEventSlim gate = new ManualResetEventSlim(false);

        readonly List<MetricsDiffHolder> counters = new List<MetricsDiffHolder>();

        public DashboardStatisticsService(IMemoryCache memoryCache, IWorkspacesCache workspaces)
        {
            this.workspaces = workspaces;
            this.memoryCache = memoryCache as MemoryCache;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                do
                {
                    // if there is no requests for State for 5 seconds, then pause metrics collection
                    if ((DateTime.UtcNow - lastQuery) > TimeSpan.FromSeconds(5))
                    {
                        gate.Reset();
                    }

                    gate.Wait(stoppingToken);

                    if (stoppingToken.IsCancellationRequested) break;

                    state = await CollectMetrics();
                } while (!stoppingToken.IsCancellationRequested);
            }, stoppingToken);
        }

        public ServerStatusResponse GetState()
        {
            lastQuery = DateTime.UtcNow;
            gate.Set();
            return state;
        }

        async Task<ServerStatusResponse> CollectMetrics()
        {
            await MetricsRegistry.Update();
            var result = new List<MetricState>();

            // collecting TotalProcessorTime change per second
            var cpuDiff = RegisterCounter(() => Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds);

            // interviews cached/evicted change over time per second
            var interviewsCached = RegisterCounter(() => (CoreMetrics.StatefullInterviewsCached as Counter).GetSummForLabels(CacheAddedLabel));
            var interviewsEvicted = RegisterCounter(() => (CoreMetrics.StatefullInterviewsCached as Counter).GetSummForLabels(CacheRemovedLabel));

            // npgsql data transfer change over time  per second
            var dataTransferRead = RegisterCounter(() => CommonMetrics.NpgsqlDataCounter.GetSummForLabels(ReadDbdataLabel));
            var dataTransferWrite = RegisterCounter(() => CommonMetrics.NpgsqlDataCounter.GetSummForLabels(WriteDbdataLabel));

            var cacheItemsDiff = RegisterCounter(() => memoryCache?.Count ?? 0);

            // request counter change over time per second
            var requests = RegisterCounter(() => HeadquartersHttpMetricsMiddleware.HttpRequestsTotal.Value);

            // collect all registered counters change over time
            await CollectMetricsDiff(TimeSpan.FromSeconds(2));

            var cpuUsage = await cpuDiff;
            result.Add(new MetricState("CPU Usage", $"{cpuUsage.ToStr("0.##%")} on " +
                    $"{"vCPU".ToQuantity(Environment.ProcessorCount)} " +
                    $"({(cpuUsage / Environment.ProcessorCount).ToStr("0.##%")} of total)", 
                        cpuUsage / Environment.ProcessorCount));

            result.Add(new MetricState("Working Memory usage", Process.GetCurrentProcess().WorkingSet64.Bytes().Humanize("0.0"),
                Process.GetCurrentProcess().WorkingSet64));

            result.Add(new MetricState(
                "Database size", DatabaseStatsCollector.DatabaseSize.Value.Bytes().Humanize("0.0"),
                DatabaseStatsCollector.DatabaseSize.Value
            ));

            // web interview
            var connections = CommonMetrics.WebInterviewConnection.GetDiffForLabels(OpenConnectionsLabel, ClosedConnectionsLabel);
            result.Add(new MetricState("Web interview connections", $"{connections}", connections));

            // stateful interviews
            var statefulInterviews = (CoreMetrics.StatefullInterviewsCached as Counter).GetDiffForLabels(CacheAddedLabel, CacheRemovedLabel);

            result.Add(new MetricState("Stateful interviews in cache", "interview".ToQuantity(statefulInterviews, "N0")
                    + $" (cached {await interviewsCached:N2} interviews/s; evicted {await interviewsEvicted:N2} interviews/s)", statefulInterviews));

            // exceptions
            result.Add(new MetricState("Exceptions count", "exception".ToQuantity(CommonMetrics.ExceptionsOccur.Value), CommonMetrics.ExceptionsOccur.Value));

            // npgsql
            var idle = CommonMetrics.NpgsqlConnections.GetSummForLabels(IdleDbConnectionsLabel);
            var busy = CommonMetrics.NpgsqlConnections.GetSummForLabels(BusyDbConnectionsLabel);

            result.Add(new MetricState("Database connections", $"Busy: {busy}, Idle: {idle}", busy));
            
            // ReSharper disable once UseStringInterpolation
            var readRate = await dataTransferRead;
            var writeRate = await dataTransferWrite;

            result.Add(new MetricState("Database Network usage", string.Format("Read: {0} ({2}), Write: {1} ({3})",
                CommonMetrics.NpgsqlDataCounter.GetSummForLabels(ReadDbdataLabel).Bytes().Humanize("0.0"),
                CommonMetrics.NpgsqlDataCounter.GetSummForLabels(WriteDbdataLabel).Bytes().Humanize("0.0"),
                readRate.Bytes().Per(TimeSpan.FromSeconds(1)).Humanize("0.0"),
                writeRate.Bytes().Per(TimeSpan.FromSeconds(1)).Humanize("0.0")
            ), readRate + writeRate));

            result.Add(new MetricState("Requests", "requests".ToQuantity(await requests, "N2") + "/s", await requests));

            result.Add(new MetricState("Thread Pool: ", $"{ThreadPool.PendingWorkItemCount} Pending Work Items", ThreadPool.PendingWorkItemCount));

            await cacheItemsDiff;

            result.Add(new MetricState("Cache", $"{memoryCache.Count} items ({cacheItemsDiff.Result:N2} items/s)", memoryCache.Count));
            
            foreach (var workspace in workspaces.AllEnabledWorkspaces())
            {
                result.Add(new MetricState($"Workspace [{workspace.Name}]", workspace.DisplayName, 0));

                var eventsCount = DatabaseStatsCollector.DatabaseTableRowsCount.Labels("events", workspace.Name).Value;
                var eventsSize = DatabaseStatsCollector.DatabaseTableSize.Labels("events", workspace.Name).Value.Bytes()
                    .Humanize("0.0");
                var interviews = DatabaseStatsCollector.DatabaseTableRowsCount.Labels("interviewsummaries", workspace.Name).Value;

                var completedEmailCount = DatabaseStatsCollector.DatabaseTableRowsCount.Labels("completedemailrecords", workspace.Name).Value;
                
                result.Add(new MetricState(
                    $"- Events",
                    $"{eventsSize} of {"event".ToQuantity(eventsCount, "N0")} for {"interview".ToQuantity(interviews, "N0")}",
                    eventsCount));

                result.Add(new MetricState(
                    $"- Workspace data size", DatabaseStatsCollector.WorkspaceSize.Labels(workspace.Name).Value.Bytes().Humanize("0.0"),
                    DatabaseStatsCollector.WorkspaceSize.Value
                ));

                result.Add(new MetricState(
                    $"- Completed emails queue size", $"{completedEmailCount}",
                    completedEmailCount));
            }
            
            return new ServerStatusResponse
            {
                LastUpdateTime = DateTime.UtcNow,
                Metrics = result
            };
        }

        private static readonly string[] OpenConnectionsLabel = { "open" };
        private static readonly string[] ClosedConnectionsLabel = { "closed" };
        private static readonly string[] CacheAddedLabel = { "added" };
        private static readonly string[] CacheRemovedLabel = { "removed" };
        private static readonly string[] IdleDbConnectionsLabel = { "idle" };
        private static readonly string[] BusyDbConnectionsLabel = { "busy" };
        private static readonly string[] ReadDbdataLabel = { "read" };
        private static readonly string[] WriteDbdataLabel = { "write" };

        public Task<double> RegisterCounter(Func<double> get)
        {
            var counter = new MetricsDiffHolder
            {
                MetricValueFactory = get,
                Result = new TaskCompletionSource<double>()
            };

            counters.Add(counter);
            return counter.Result.Task;
        }

        public async Task CollectMetricsDiff(TimeSpan timeSpan)
        {
            var sw = Stopwatch.StartNew();

            foreach (var data in counters)
            {
                data.InitialValue = data.MetricValueFactory();
            }

            await Task.Delay(timeSpan);

            sw.Stop();

            foreach (var data in counters)
            {
                var current = data.MetricValueFactory();

                data.Result.SetResult((current - data.InitialValue) / sw.Elapsed.TotalSeconds);
            }

            counters.Clear();
        }


        private class MetricsDiffHolder
        {
            public Func<double> MetricValueFactory { get; set; }
            public TaskCompletionSource<double> Result { get; set; }
            public double InitialValue { get; set; }
        }
    }
}
