#nullable enable
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Autofac;
using Autofac.Core.Lifetime;
using Microsoft.Extensions.Logging;
using NHibernate;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    [DebuggerDisplay("Id#{Id}; SessionId: {SessionId}")]
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly Lazy<ISessionFactory> sessionFactory;
        private readonly ILogger<UnitOfWork> logger;
        private bool isDisposed = false;
        private bool shouldAcceptChanges = false;
        private bool shouldDiscardChanges = false;
        private readonly bool rootScopeExecution = false;
        private static long counter = 0;
        public long Id { get; }
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public UnitOfWork(
            Lazy<ISessionFactory> sessionFactory,
            ILogger<UnitOfWork> logger, 
            IWorkspaceContextAccessor workspaceContextAccessor,
            ILifetimeScope scope)
        {
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));

            if (scope.Tag == LifetimeScope.RootTag)
            {
                logger.LogError("UnitOfWork should not be created in root scope.");
                rootScopeExecution = true;
                // throw new ArgumentException("Unit of work cannot be resoled in root scope");
                // it's not helpful to throw exception here, as there will be no clue on which code 
                // caused an error
                // Will throw later with ObjectDisposedException
            }

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

        readonly ConcurrentDictionary<string, (ISession session, ITransaction transaction)> unitOfWorks = new();

        public ISession Session
        {
            get
            {
                if(rootScopeExecution)
                {
                    logger.LogError("Error getting session. Unit of work Id: {UnitOfWorkId} Thread:{threadId}", Id, Thread.CurrentThread.ManagedThreadId);
                    throw new RootScopeResolveException("UnitOfWork should not be resolved from Root lifetime scope");
                }

                if (isDisposed)
                {
                    logger.LogError("Error getting session. Unit of work Id: {UnitOfWorkId} Thread:{threadId}", Id, Thread.CurrentThread.ManagedThreadId);
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }

                var ws = this.workspaceContextAccessor.CurrentWorkspace();

                var unitOfWork = unitOfWorks.GetOrAdd(ws?.Name ?? WorkspaceConstants.SchemaName, workspace =>
                {
                    var session = sessionFactory.Value.OpenSession();
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
