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

        private readonly Gauge sessionCloseCount = new Gauge(@"nhibernate_session_close_count", @"Count of closed sessions", "source");
        private readonly Gauge sessionOpenCount = new Gauge(@"nhibernate_session_open_count", @"Count of opened sessions", "source");

        public void RegisterMetrics()
        {
        }

        public void UpdateMetrics()
        {
            var sessionFactory = this.serviceLocator.GetInstance<ISessionFactory>();
            this.sessionCloseCount.Set(sessionFactory.Statistics.SessionCloseCount);
            this.sessionOpenCount.Set(sessionFactory.Statistics.SessionOpenCount);
        }
    }
}
