using NHibernate;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class NHibernateStatsCollector : IOnDemandCollector
    {
        private readonly ISessionFactory sessionFactory;
        private readonly string name;

        public NHibernateStatsCollector(string name, ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            this.name = name;
        }

        private Gauge sessionCloseCount;
        private Gauge sessionOpenCount;

        public void RegisterMetrics()
        {
            sessionCloseCount = new Gauge(@"nhibernate_session_close_count", @"Count of closed sessions", "source");
            sessionOpenCount =new Gauge(@"nhibernate_session_open_count", @"Count of opened sessions", "source");
        }

        public void UpdateMetrics()
        {
            this.sessionCloseCount.Labels(name).Set(sessionFactory.Statistics.SessionCloseCount);
            this.sessionOpenCount.Labels(name).Set(sessionFactory.Statistics.SessionOpenCount);
        }
    }
}
