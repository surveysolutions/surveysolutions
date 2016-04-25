using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal interface ISessionProvider
    {
        ISession GetSession();
        string GetEntityIdentifierColumnName(Type entityType);
    }

    internal interface IPlainSessionProvider : ISessionProvider { }
}