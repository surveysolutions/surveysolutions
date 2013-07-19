using System;

using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    #warning TLK: make string identifiers here after switch to new storage
    public abstract class RavenReadSideRepositoryAccessor<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly DocumentStore ravenStore;

        protected RavenReadSideRepositoryAccessor(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        private static string ViewName
        {
            get { return typeof(TEntity).FullName; }
        }

        protected IDocumentSession OpenSession()
        {
            this.ravenStore.DatabaseCommands.EnsureDatabaseExists("Views");
            return this.ravenStore.OpenSession("Views");
        }

        protected int MaxNumberOfRequestsPerSession
        {
            get { return this.ravenStore.Conventions.MaxNumberOfRequestsPerSession; }
        }

        protected static string ToRavenId(Guid id)
        {
            return string.Format("{0}${1}", ViewName, id.ToString());
        }
    }
}