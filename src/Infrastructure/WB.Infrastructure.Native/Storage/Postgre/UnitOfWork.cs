using System;
using System.Diagnostics;
using System.Threading;
using NHibernate;
using NHibernate.Impl;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

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
        public Guid? sessionId;

        private readonly ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();

        public UnitOfWork(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            Id = Interlocked.Increment(ref Counter);

            if (session != null) throw new InvalidOperationException("Unit of work already started");
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));

            session = this.sessionFactory.OpenSession();
            transaction = session.BeginTransaction();
            sessionId = (session as SessionImpl)?.SessionId;

            //logger.Info($"creating UOW:{Id} sessionId:{(session as SessionImpl)?.SessionId} Thread:{Thread.CurrentThread.ManagedThreadId}");
        }

        public void AcceptChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));

            transaction.Commit();
            session.Close();
            transaction.Dispose();
            session.Dispose();
            //logger.Info($"session closing UOW:{Id} sessionId:{(session as SessionImpl)?.SessionId} Thread:{Thread.CurrentThread.ManagedThreadId}");
            transaction = null;
            session = null;
        }

        public ISession Session
        {
            get
            {
                if (isDisposed)
                {
                    logger.Info($"Error getting session. UOW:{Id} old sessionId:{sessionId} Thread:{Thread.CurrentThread.ManagedThreadId}");
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }
                return session;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            transaction?.Rollback();
            session?.Close();
            transaction?.Dispose();
            session?.Dispose();

            //logger.Info($"session closing in dispose UOW:{Id} sessionId:{(session as SessionImpl)?.SessionId} Thread:{Thread.CurrentThread.ManagedThreadId}");

            transaction = null;
            session = null;

            isDisposed = true;
        }
    }
}
