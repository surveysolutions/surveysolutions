using NHibernate;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal interface ISessionProvider
    {
        ISession GetSession();
    }

    internal interface IPlainSessionProvider : ISessionProvider { }
}