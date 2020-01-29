using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Impl;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    [DebuggerDisplay("Id#{Id}; SessionId: {SessionId}")]
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ISessionFactory sessionFactory;
        private readonly ILogger logger;
        private ISession session;
        private ITransaction transaction;
        private bool isDisposed = false;
        private bool shouldAcceptChanges = true;
        public Guid? SessionId;
        private static long counter = 0;
        public long Id { get; }
        
        public UnitOfWork(ISessionFactory sessionFactory, ILogger logger)
        {
            if (session != null) throw new InvalidOperationException("Unit of work already started");
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));
            this.sessionFactory = sessionFactory;
            this.logger = logger;
            Id = Interlocked.Increment(ref counter);
        }

        public void AcceptChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        public void DiscardChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
            shouldAcceptChanges = false;
        }

        public async Task AcceptChangesAsync()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
            if (this.session != null)
            {
                await this.session.FlushAsync().ConfigureAwait(false);
            }

            if(transaction?.IsActive == true)
                await transaction.CommitAsync().ConfigureAwait(false);
        }

        public ISession Session
        {
            get
            {
                if (isDisposed)
                {
                    logger.Info($"Error getting session. Old sessionId:{SessionId} Thread:{Thread.CurrentThread.ManagedThreadId}");
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }

                if (this.session == null)
                {
                    session = sessionFactory.OpenSession();
                    transaction = session.BeginTransaction(IsolationLevel.ReadCommitted);
                    SessionId = (session as SessionImpl)?.SessionId;
                }

                return session;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (transaction?.IsActive == true)
            {
                if (shouldAcceptChanges)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }

            transaction?.Dispose();
            session?.Dispose();

            isDisposed = true;
        }
    }
}
