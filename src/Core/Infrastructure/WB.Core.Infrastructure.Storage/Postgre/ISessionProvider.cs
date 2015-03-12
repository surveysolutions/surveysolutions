using NHibernate;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public interface ISessionProvider
    {
        ISession GetSession();
    }
}