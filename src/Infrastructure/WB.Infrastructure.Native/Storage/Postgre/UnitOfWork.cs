#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
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
        private readonly ILogger<UnitOfWork> logger;
        
        private bool shouldAcceptChanges = false;
        private bool shouldDiscardChanges = false;
        private bool completionAttempted;
        private bool completionSucceeded;
        private readonly bool rootScopeExecution = false;
        private static long counter = 0;
        public long Id { get; }
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        private int disposeCount;

        public UnitOfWork(
            ILogger<UnitOfWork> logger, 
            IWorkspaceContextAccessor workspaceContextAccessor,
            ILifetimeScope scope)
        {
            if (disposeCount > 0) throw new ObjectDisposedException(nameof(UnitOfWork));

            if (scope.Tag == LifetimeScope.RootTag)
            {
                logger.LogError("UnitOfWork should not be created in root scope.");
                rootScopeExecution = true;
                // throw new ArgumentException("Unit of work cannot be resoled in root scope");
                // it's not helpful to throw exception here, as there will be no clue on which code 
                // caused an error
                // Will throw later with ObjectDisposedException
            }
            this.logger = logger;
            this.workspaceContextAccessor = workspaceContextAccessor;
            Id = Interlocked.Increment(ref counter);
            this.scope = scope;
        }

        public void AcceptChanges()
        {
            EnsureCanChange();
            shouldAcceptChanges = true;
        }

        public void DiscardChanges()
        {
            EnsureCanChange();
            shouldDiscardChanges = true;
        }

        private void EnsureCanChange()
        {
            if (disposeCount > 0) throw new ObjectDisposedException(nameof(UnitOfWork));
            if (completionAttempted) throw new InvalidOperationException("Unit of work transaction completion has already been attempted.");
        }

        public void Complete()
        {
            if (disposeCount > 0) throw new ObjectDisposedException(nameof(UnitOfWork));
            if (completionSucceeded) return;
            EnsureCanChange();

            CompleteTransactions();

            // Result filters, views and lazy-loaded properties may still need these sessions.
            // A separate read-only transaction prevents rendering from introducing late writes.
            foreach (var entry in unitOfWorks)
            {
                entry.Value.transaction.Dispose();
                var transaction = BeginTransaction(entry.Value.session, readOnly: true);
                unitOfWorks[entry.Key] = (entry.Value.session, transaction);
            }

            completionSucceeded = true;
        }

        private void CompleteTransactions()
        {
            // Set before the first commit: a failed/ambiguous commit must never be retried by Dispose.
            completionAttempted = true;
            foreach (var (_, transaction) in unitOfWorks.Values)
            {
                if (!transaction.IsActive) continue;
                if (shouldAcceptChanges && !shouldDiscardChanges)
                    transaction.Commit();
                else
                    transaction.Rollback();
            }
        }

        private static ITransaction BeginTransaction(ISession session, bool readOnly)
        {
            if (readOnly)
            {
                session.FlushMode = FlushMode.Manual;
                session.DefaultReadOnly = true;
            }

            var transaction = session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                // DefaultReadOnly alone does not prevent inserts, deletes or explicit SQL writes.
                if (readOnly)
                    session.CreateSQLQuery("SET TRANSACTION READ ONLY").ExecuteUpdate();
                return transaction;
            }
            catch
            {
                transaction.Dispose();
                throw;
            }
        }

        readonly ConcurrentDictionary<string, (ISession session, ITransaction transaction)> unitOfWorks = new();
        private readonly ILifetimeScope scope;

        public ISession Session
        {
            get
            {
                if(rootScopeExecution)
                {
                    logger.LogError("Error getting session. Unit of work Id: {UnitOfWorkId} Thread:{threadId}", Id, Thread.CurrentThread.ManagedThreadId);
                    throw new RootScopeResolveException("UnitOfWork should not be resolved from Root lifetime scope");
                }

                if (disposeCount > 0)
                {
                    logger.LogError("Error getting session. Unit of work is disposed. Id: {UnitOfWorkId} Thread:{threadId}", Id, Thread.CurrentThread.ManagedThreadId);
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }

                if (completionAttempted && !completionSucceeded)
                    throw new InvalidOperationException("Unit of work transaction completion failed.");

                var ws = this.workspaceContextAccessor.CurrentWorkspace();

                var unitOfWork = unitOfWorks.GetOrAdd(ws?.Name ?? WorkspaceConstants.SchemaName, workspace =>
                {
                    //resolving when needed but not when injected
                    var session = scope.Resolve<Lazy<ISessionFactory>>().Value.OpenSession();
                    try
                    {
                        var transaction = BeginTransaction(session, readOnly: completionSucceeded);
                        return (session, transaction);
                    }
                    catch
                    {
                        session.Dispose();
                        throw;
                    }
                });

                return unitOfWork.session;
            }
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposeCount) == 1)
            {
                var errors = new List<Exception>();
                if (!completionAttempted)
                {
                    // Preserve deferred completion for non-HTTP callers.
                    try
                    {
                        CompleteTransactions();
                    }
                    catch (Exception exception)
                    {
                        errors.Add(exception);
                    }
                }

                foreach (var (session, transaction) in unitOfWorks.Values)
                {
                    try
                    {
                        try
                        {
                            if (transaction.IsActive)
                                transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        errors.Add(exception);
                    }

                    try
                    {
                        session.Dispose();
                    }
                    catch (Exception exception)
                    {
                        errors.Add(exception);
                    }
                }

                if (errors.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
                if (errors.Count > 1) throw new AggregateException(errors);
            }
        }
    }
}
