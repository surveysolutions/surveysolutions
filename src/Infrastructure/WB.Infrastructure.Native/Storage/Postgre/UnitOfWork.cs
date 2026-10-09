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
        private readonly ILogger<UnitOfWork> logger;
        
        private bool shouldAcceptChanges = false;
        private bool shouldDiscardChanges = false;
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
            if (disposeCount > 0) throw new ObjectDisposedException(nameof(UnitOfWork));
            shouldAcceptChanges = true;
        }

        public void DiscardChanges()
        {
            if (disposeCount > 0) throw new ObjectDisposedException(nameof(UnitOfWork));
            shouldDiscardChanges = true;
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

                var ws = this.workspaceContextAccessor.CurrentWorkspace();
                var key = ws?.Name ?? WorkspaceConstants.SchemaName;

                if (unitOfWorks.TryGetValue(key, out var existing))
                    return existing.session;

                // ConcurrentDictionary.GetOrAdd may invoke the factory more than once under contention,
                // which would open a session + transaction (i.e. a DB connection) that is never disposed.
                lock (sessionLock)
                {
                    if (disposeCount > 0)
                        throw new ObjectDisposedException(nameof(UnitOfWork));

                    if (unitOfWorks.TryGetValue(key, out existing))
                        return existing.session;

                    //resolving when needed but not when injected
                    var session = scope.Resolve<Lazy<ISessionFactory>>().Value.OpenSession();
                    try
                    {
                        var transaction = session.BeginTransaction(IsolationLevel.ReadCommitted);
                        unitOfWorks[key] = (session, transaction);
                    }
                    catch
                    {
                        session.Dispose();
                        throw;
                    }

                    return session;
                }
            }
        }

        private readonly object sessionLock = new();

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposeCount) == 1)
            {
                (ISession session, ITransaction transaction)[] items;
                lock (sessionLock)
                {
                    items = new (ISession, ITransaction)[unitOfWorks.Count];
                    unitOfWorks.Values.CopyTo(items, 0);
                    unitOfWorks.Clear();
                }

                Exception? firstError = null;
                var commitRemaining = shouldAcceptChanges && !shouldDiscardChanges;

                foreach (var (session, transaction) in items)
                {
                    try
                    {
                        if (transaction.IsActive == true)
                        {
                            if (commitRemaining)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Clean up all workspaces, but do not commit more writes after a failure.
                        commitRemaining = false;
                        firstError ??= e;
                    }
                    finally
                    {
                        try { transaction.Dispose(); }
                        catch (Exception e)
                        {
                            commitRemaining = false;
                            firstError ??= e;
                        }

                        try { session.Dispose(); }
                        catch (Exception e)
                        {
                            commitRemaining = false;
                            firstError ??= e;
                        }
                    }
                }

                if (firstError != null)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(firstError).Throw();
            }
        }
    }
}
