using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal abstract class AbstractSessionProvider : ISessionProvider, IDisposable
    {
        private readonly ISessionFactory sessionFactory;
        protected Lazy<ISession> lazySession;

        protected AbstractSessionProvider(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public ISession GetSession()
        {
            ISession session = this.lazySession?.Value;

            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. " +
                                                    "Make sure to call BeginTransaction before getting session instance.");

            return session;
        }

        public bool TransactionStarted => this.lazySession?.Value?.Transaction?.IsActive ?? false;

        protected void CreateSession()
        {
            if (this.lazySession != null)
                throw new InvalidOperationException();

            this.lazySession = new Lazy<ISession>(() => this.sessionFactory.OpenSession(), true);
        }

        public void Dispose()
        {
            if (this.lazySession?.IsValueCreated == true)
            {
                this.lazySession.Value.Dispose();
            }

            this.lazySession = null;

            GC.SuppressFinalize(this);
        }
    }
}