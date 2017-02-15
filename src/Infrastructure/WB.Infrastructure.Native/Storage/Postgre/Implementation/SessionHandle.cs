using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public class SessionHandle : IDisposable
    {
        public SessionHandle(ISession session, ITransaction transaction)
        {
            this.Session = session;
            this.Transaction = transaction;
        }

        public ISession Session { get; }
        public ITransaction Transaction { get; }

        public void Dispose()
        {
            this.Transaction.Dispose();
            this.Session.Dispose();
        }
    }
}