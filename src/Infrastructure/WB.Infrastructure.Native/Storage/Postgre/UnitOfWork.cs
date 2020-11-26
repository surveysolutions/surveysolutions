#nullable enable
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using NHibernate;
using WB.Infrastructure.Native.Workspaces;

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
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public UnitOfWork(ISessionFactory sessionFactory,
            ILogger<UnitOfWork> logger, IWorkspaceContextAccessor workspaceContextAccessor)
        {
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));
            this.sessionFactory = sessionFactory;
            this.logger = logger;
            this.workspaceContextAccessor = workspaceContextAccessor;
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

                var ws = this.workspaceContextAccessor.CurrentWorkspace();

                var unitOfWork = unitOfWorks.GetOrAdd(ws.Name, workspace =>
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
