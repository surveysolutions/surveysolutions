using NHibernate;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class NHibernateStatsCollector : IOnDemandCollector
    {
        private readonly IServiceLocator serviceLocator;
        
        public NHibernateStatsCollector(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        private readonly Gauge sessionCountTotal = new Gauge(@"nhibernate_sessions_total", @"Count of closed sessions","state");

        public void RegisterMetrics()
        {
        }

        public void UpdateMetrics()
        {
            var sessionFactory = this.serviceLocator.GetInstance<ISessionFactory>();
            this.sessionCountTotal.Labels("closed").Set(sessionFactory.Statistics.SessionCloseCount);
            this.sessionCountTotal.Labels("open").Set(sessionFactory.Statistics.SessionOpenCount);
        }
    }
}
