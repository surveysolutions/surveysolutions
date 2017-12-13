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
        }

        public void RegisterMetrics()
        {
            throttle.Start();
        }

        public void UpdateMetrics()
        {
            if (throttle.Elapsed <= TimeSpan.FromMinutes(5)) return;

            using (var session = sessionFactory.OpenStatelessSession())
            {
                var packagesCount = session.Query<BrokenInterviewPackage>().Count();
                CommonMetrics.BrokenPackagesCount.Set(packagesCount);
            }

            throttle.Restart();
        }
    }
}