using System;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public interface IUnitOfWork : IDisposable
    {
        void AcceptChanges();
        void DiscardChanges();

        /// <summary>
        /// Commits accepted changes (or rolls back discarded/unaccepted changes) immediately.
        /// Keeps sessions open for read-only access until disposal. A failed completion cannot be retried.
        /// Call before executing an HTTP result; AcceptChanges alone defers completion until disposal.
        /// </summary>
        void Complete();

        ISession Session { get; }
    }
}
