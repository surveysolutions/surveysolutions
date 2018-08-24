using System;
using System.Diagnostics;
using System.Threading;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    [DebuggerDisplay("Id = {Id}")]
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ISessionFactory sessionFactory;
        private ISession session;
        private ITransaction transaction;
        private bool isDisposed = false;
        private static int Counter = 0;
        public int Id { get; set; }

        public UnitOfWork(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            Id = Interlocked.Increment(ref Counter);

            if (session != null) throw new InvalidOperationException("Unit of work already started");
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));

            session = this.sessionFactory.OpenSession();
            transaction = session.BeginTransaction();
        }

        public void AcceptChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));

            transaction.Commit();
            session.Close();
            transaction = null;
            session = null;
        }

        public ISession Session
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
                return session;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (transaction != null)
            {
                transaction.Rollback();
                transaction = null;
            }

            if (session != null)
            {
                session.Close();
                session = null;
            }

            isDisposed = true;
        }
    }
}
