using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public interface ISessionProvider
    {
        ISession GetSession();
    }

    internal interface IPlainSessionProvider : ISessionProvider { }
}