#nullable enable
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Impl;
using Npgsql;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    [DebuggerDisplay("Id#{Id}; SessionId: {SessionId}")]
    public sealed class UnitOfWork : IUnitOfWork
    {
        private static ConcurrentDictionary<string, string> connectionStringCache = new ConcurrentDictionary<string, string>();
        
        private readonly ISessionFactory sessionFactory;
        private readonly ILogger<UnitOfWork> logger;
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        private readonly IWorkspaceNameProvider workspaceNameProvider;
        private ISession? session;
        private ITransaction? transaction;
        private bool isDisposed = false;
        private bool shouldAcceptChanges = false;
        private bool shouldDiscardChanges = false;
        public Guid? SessionId;
        private static long counter = 0;
        public long Id { get; }
        
        public UnitOfWork(ISessionFactory sessionFactory, 
            ILogger<UnitOfWork> logger,
            UnitOfWorkConnectionSettings connectionSettings,
            IWorkspaceNameProvider workspaceNameProvider)
        {
            if (session != null) throw new InvalidOperationException("Unit of work already started");
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));
            this.sessionFactory = sessionFactory;
            this.logger = logger;
            this.connectionSettings = connectionSettings;
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

        public ISession Session
        {
            get
            {
                if (isDisposed)
                {
                    logger.LogInformation("Error getting session. Old SessionId:{SessionId} Thread:{ManagedThreadId}", SessionId, Thread.CurrentThread.ManagedThreadId);
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                }
                
                if (this.session == null)
                {
                    string connectionString = connectionStringCache.GetOrAdd(
                        this.workspaceNameProvider.CurrentWorkspace(),
                        key =>
                        {
                            NpgsqlConnectionStringBuilder connectionStringBuilder =
                                new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString)
                                {
                                    SearchPath = key
                                };

                            return connectionStringBuilder.ToString();
                        });
                    
                    var connection = new NpgsqlConnection(connectionString);
                    connection.Open();
                    session = sessionFactory.WithOptions()
                        .Connection(connection)
                        .OpenSession();
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
                if (shouldAcceptChanges && !shouldDiscardChanges)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }

            transaction?.Dispose();
            session?.Close().Close();

            isDisposed = true;
        }
    }
}
