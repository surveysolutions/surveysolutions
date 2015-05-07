using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PostgresClientBatchingBatcherFactory : IBatcherFactory
    {
        public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
        {
            return new PostgresClientBatchingBatcher(connectionManager, interceptor);
        }
    }
}