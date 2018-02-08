using System;
using System.Diagnostics;
using System.Linq;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.Views;
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
                if (throttle.Elapsed <= TimeSpan.FromMinutes(1)) return;

                using (var session = sessionFactory.OpenStatelessSession())
                {
                    var packages = from bip in session.Query<BrokenInterviewPackage>()
                        group bip by bip.ExceptionType
                        into g
                        select new {Type = g.Key, Count = g.Count()};

                    foreach (var package in packages.ToList())
                    {
                        CommonMetrics.BrokenPackagesCount.Labels(package.Type).Set(package.Count);
                    }
                }

            throttle.Restart();
            }
        }
    }
}