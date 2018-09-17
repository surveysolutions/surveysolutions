using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public interface IUnitOfWork : IDisposable
    {
        void AcceptChanges();
        ISession Session { get; }

        [Obsolete]
        T ExecuteInQueryTransaction<T>(Func<T> func);
        [Obsolete]
        T ExecuteInPlainTransaction<T>(Func<T> func);
        [Obsolete]
        void ExecuteInPlainTransaction(Action action);
    }
}
