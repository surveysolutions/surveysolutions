using System;
using System.Diagnostics;
using System.Linq;
using NHibernate;
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
                var packagesCount = session.CreateSQLQuery(@"
                    select exceptiontype, count(exceptiontype) 
                    from plainstore.brokeninterviewpackages 
                    group by exceptiontype");
                
                foreach (var package in packagesCount.List().Cast<object[]>())
                {
                    CommonMetrics.BrokenPackagesCount.Labels((string)package[0]).Set((long)package[1]);
                }
            }

            throttle.Reset();
        }
    }
}