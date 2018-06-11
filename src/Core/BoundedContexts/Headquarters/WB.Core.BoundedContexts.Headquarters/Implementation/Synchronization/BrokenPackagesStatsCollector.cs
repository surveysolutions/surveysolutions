using System;
using System.Diagnostics;
using System.Linq;
using Dapper;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class BrokenPackagesStatsCollector : IOnDemandCollector
    {
        private readonly ISessionFactory sessionFactory;
        private readonly Stopwatch throttle = new Stopwatch();

        public BrokenPackagesStatsCollector(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            throttle.Start();
        }

        public void RegisterMetrics()
        {
            throttle.Start();
        }

        public void UpdateMetrics()
        {
            lock (throttle)
            {
                if (throttle.Elapsed <= TimeSpan.FromSeconds(30)) return;

                using (var session = sessionFactory.OpenStatelessSession())
                {
                    var packages = from bip in session.Query<BrokenInterviewPackage>()
                       group bip by bip.ExceptionType into g
                       select new { Type = g.Key, Count = g.Count() };

                    foreach (var type in Enum.GetValues(typeof(InterviewDomainExceptionType)))
                    {
                        BrokenPackagesCount.Labels(type.ToString()).Set(0);
                    }

                    BrokenPackagesCount.Labels("Unexpected").Set(0);

                    foreach (var package in packages.ToList())
                    {
                        BrokenPackagesCount.Labels(package.Type).Set(package.Count);
                    }

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
                }

                throttle.Restart();
            }
        }
        
        private static readonly Gauge BrokenPackagesCount = new Gauge(
            "wb_broken_packages_count",
            "Amount of broken packages on server", "type");

        private static readonly Gauge DatabaseTableRowsCount = new Gauge(
            "wb_table_estimated_rows_count",
            "Amount of rows in table", "table");

        private static readonly Gauge DatabaseTableSize = new Gauge(
            "wb_table_estimated_size_bytes",
            "Size of the table in bytes", "table");
    }
}
