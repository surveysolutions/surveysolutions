using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class BrokenPackagesStatsCollector : IOnDemandCollector
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger<BrokenPackagesStatsCollector> logger;
        private readonly Stopwatch throttle = new Stopwatch();

        public BrokenPackagesStatsCollector(IServiceLocator serviceLocator, ILogger<BrokenPackagesStatsCollector> logger)
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
                var packages = from bip in session.Query<BrokenInterviewPackage>()
                               group bip by bip.ExceptionType into g
                               select new { Type = g.Key, Count = g.Count() };

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var type in Enum.GetValues(typeof(InterviewDomainExceptionType)))
                {
                    BrokenPackagesCount.Labels(type.ToString()).Set(0);
                }

                BrokenPackagesCount.Labels("Unexpected").Set(0);

                foreach (var package in packages.ToList())
                {
                    BrokenPackagesCount.Labels(package.Type).Set(package.Count);
                }
                cancellationToken.ThrowIfCancellationRequested();
                var sizesData = session.Connection.Query<(string name, long rows, long size)>(
                    @"SELECT relname AS table_name, c.reltuples AS row_estimate, 
                            pg_total_relation_size(c.oid) AS total_bytes									
                          FROM pg_class c
                          where relname in ('events', 'interviews', 'interviewsummaries');");
                
                foreach (var data in sizesData)
                {
                    DatabaseTableRowsCount.Labels(data.name).Set(data.rows);
                    DatabaseTableSize.Labels(data.name).Set(data.size);
                }

                var dbSize = session.Connection.QuerySingle<long>(@"SELECT pg_database_size(current_database())");
                DatabaseSize.Set(dbSize);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to collect broken packages information");
            }
        }

        public static readonly Gauge BrokenPackagesCount = new Gauge(
            "wb_broken_packages_count",
            "Amount of broken packages on server", "type");

        public static readonly Gauge DatabaseTableRowsCount = new Gauge(
            "wb_table_estimated_rows_count",
            "Amount of rows in table", "table");

        public static readonly Gauge DatabaseTableSize = new Gauge(
            "wb_table_estimated_size_bytes",
            "Size of the table in bytes", "table");

        public static readonly Gauge DatabaseSize = new Gauge(
            "wb_database_size_bytes",
            "Size of DB in bytes");
    }
}
