using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal interface ISessionProvider
    {
        ISession GetSession();
    }

    internal interface IPlainSessionProvider : ISessionProvider { }
}