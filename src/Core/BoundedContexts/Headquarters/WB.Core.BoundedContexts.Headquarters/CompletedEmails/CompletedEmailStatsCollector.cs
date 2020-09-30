using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.CompletedEmails
{
    public class CompletedEmailStatsCollector : IOnDemandCollector
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger<CompletedEmailStatsCollector> logger;
        private readonly Stopwatch throttle = new Stopwatch();

        public CompletedEmailStatsCollector(IServiceLocator serviceLocator, ILogger<CompletedEmailStatsCollector> logger)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
        }
        public void RegisterMetrics() { }

        public void UpdateMetrics()
        {
            lock (throttle)
            {
                if (throttle.IsRunning && throttle.Elapsed <= TimeSpan.FromSeconds(30)) return;
                throttle.Restart();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

                try
                {
                    var task = Task.Run(() => Update(cts.Token));//you can pass parameters to the method as well

                    if (task.Wait(TimeSpan.FromSeconds(2)))
                        return; //the method returns elegantly
                    else
                        return; //the method timed-out    
                } catch {  /* om om om */}
            }
        }
        
        private void Update(CancellationToken cancellationToken)
        {
            try
            {
                using var session = serviceLocator.GetInstance<ISessionFactory>().OpenStatelessSession();
                cancellationToken.ThrowIfCancellationRequested();

                var tableRows = session.Connection.Query<(string name, long rows, long size)>(
                    @"SELECT relname AS table_name, c.reltuples AS row_estimate, 
                            pg_total_relation_size(c.oid) AS total_bytes									
                          FROM pg_class c
                          where relname in ('completedemailrecords');");
                
                foreach (var table in tableRows)
                {
                    DatabaseTableRowsCount.Labels(table.name).Set(table.rows);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to collect completed email information");
            }
        }

        public static readonly Gauge DatabaseTableRowsCount = new Gauge(
            "wb_table_estimated_rows_count",
            "Amount of rows in table", "table");
    }
}