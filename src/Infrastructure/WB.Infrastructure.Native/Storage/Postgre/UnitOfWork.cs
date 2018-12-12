using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NHibernate;
using NHibernate.Impl;
using WB.Core.GenericSubdomains.Portable;
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
        public Guid? SessionId;
        private static long counter = 0;

        public DateTime CreatedAt {get;}
        public static readonly ConcurrentDictionary<string, List<UnitOfWork>> opennedUofs = new ConcurrentDictionary<string, List<UnitOfWork>>();
        private readonly List<UnitOfWork> myList;
        public long Id { get; }
        
        public UnitOfWork(ISessionFactory sessionFactory, ILogger logger)
        {
            if (session != null) throw new InvalidOperationException("Unit of work already started");
            if (isDisposed == true) throw new ObjectDisposedException(nameof(UnitOfWork));
            this.sessionFactory = sessionFactory;
            this.logger = logger;
            Id = Interlocked.Increment(ref counter);

            StackTrace t = new StackTrace();
            var stackTrace = t.ToString();
            this.CreatedAt = DateTime.UtcNow;

            var list = opennedUofs.GetOrAdd(stackTrace, key => new List<UnitOfWork>());
            this.myList = list;
            list.Add(this);
        }

        public void AcceptChanges()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(UnitOfWork));

            transaction?.Commit();
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
                    transaction = session.BeginTransaction();
                    SessionId = (session as SessionImpl)?.SessionId;
                }

                return session;
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            this.myList.Remove(this);
            transaction?.Dispose();
            session?.Dispose();

            isDisposed = true;
        }
    }
}
