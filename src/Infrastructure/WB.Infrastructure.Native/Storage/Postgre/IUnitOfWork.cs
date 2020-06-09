using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public interface IUnitOfWork : IDisposable
    {
        void AcceptChanges();
        void DiscardChanges();
        ISession Session { get; }
    }
}
