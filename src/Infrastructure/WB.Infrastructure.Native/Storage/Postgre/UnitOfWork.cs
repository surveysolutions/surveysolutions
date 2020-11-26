#nullable enable
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    [DebuggerDisplay("Id#{Id}; SessionId: {SessionId}")]
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ISessionFactory sessionFactory;
        private readonly ILogger<UnitOfWork> logger;
        private bool isDisposed = false;
        private bool shouldAcceptChanges = false;
        private bool shouldDiscardChanges = false;
        public Guid? SessionId;
        private static long counter = 0;
        public long Id { get; }
        private readonly IWorkspaceNameProvider workspaceNameProvider;

        public UnitOfWork(ISessionFactory sessionFactory,
            ILogger<UnitOfWork> logger, IWorkspaceNameProvider workspaceNameProvider)
        {
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));
            this.sessionFactory = sessionFactory;
            this.logger = logger;
            this.workspaceNameProvider = workspaceNameProvider;
            Id = Interlocked.Increment(ref counter);
        }

        public void AcceptChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
            shouldAcceptChanges = true;
        }

        public void DiscardChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));
            shouldDiscardChanges = true;
        }

        readonly ConcurrentDictionary<string, (ISession session, ITransaction transaction)> unitOfWorks 
            = new ConcurrentDictionary<string, (ISession, ITransaction)>();

        public ISession Session
        {
            get
            {
                if (isDisposed)
                {
                    logger.LogInformation("Error getting session. Old SessionId:{SessionId} Thread:{ManagedThreadId}", SessionId, Thread.CurrentThread.ManagedThreadId);
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }

                var ws = this.workspaceNameProvider.CurrentWorkspace();

                var unitOfWork = unitOfWorks.GetOrAdd(ws, workspace =>
                {
                    var session = sessionFactory.OpenSession();
                    var transaction = session.BeginTransaction(IsolationLevel.ReadCommitted);
                    return (session, transaction);
                });

                return unitOfWork.session;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            foreach (var (session, transaction) in unitOfWorks.Values)
            {
                if (transaction.IsActive == true)
                {
                    if (shouldAcceptChanges && !shouldDiscardChanges)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }

                transaction.Dispose();
                session.Dispose();
            }

            isDisposed = true;
        }
    }
}
