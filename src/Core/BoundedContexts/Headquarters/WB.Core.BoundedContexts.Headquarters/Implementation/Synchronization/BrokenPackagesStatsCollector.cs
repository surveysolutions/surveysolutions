using System.Linq;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class BrokenPackagesStatsCollector : IOnDemandCollector
    {
        private readonly ISessionFactory sessionFactory;
        private Gauge brokenPackagesCount;

        public BrokenPackagesStatsCollector(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void RegisterMetrics()
        {
            this.brokenPackagesCount = new Gauge(
                "wb_broken_packages_count",
                "Amount of broken packages on server");
        }

        public void UpdateMetrics()
        {
            using (var session = sessionFactory.OpenStatelessSession())
            {
                var packagesCount = session.Query<BrokenInterviewPackage>().Count();
                this.brokenPackagesCount.Set(packagesCount);
            }
        }
    }
}