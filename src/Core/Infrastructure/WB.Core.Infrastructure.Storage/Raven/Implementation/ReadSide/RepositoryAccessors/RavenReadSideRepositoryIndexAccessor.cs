using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Raven.Client;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenReadSideRepositoryIndexAccessor : IReadSideRepositoryIndexAccessor
    {
        private readonly IDocumentStore ravenStore;
        private readonly Assembly[] assembliesWithIndexes;
        private bool wereIndexesCreated = false;

        public RavenReadSideRepositoryIndexAccessor(IDocumentStore ravenStore, Assembly[] assembliesWithIndexes)
        {
            this.ravenStore = ravenStore;
            this.assembliesWithIndexes = assembliesWithIndexes;
        }

        public IQueryable<TResult> Query<TResult>(string indexName)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return session.Query<TResult>(indexName);
            }
        }

        public TResult Query<TEntity, TResult>(string indexName, Func<IQueryable<TEntity>, TResult> query)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return query.Invoke(
                    session.Query<TEntity>(indexName));
            }
        }

        private IDocumentSession OpenSession()
        {
            this.EnsureIndexesExist();

            return this.ravenStore.OpenSession();
        }

        private void EnsureIndexesExist()
        {
            if (this.wereIndexesCreated)
                return;

            foreach (Assembly assembly in this.assembliesWithIndexes)
            {
                this.RegisterIndexesFromAssembly(assembly);
            }

            this.wereIndexesCreated = true;
        }

        private void RegisterIndexesFromAssembly(Assembly assembly)
        {
            IndexCreation.CreateIndexes(assembly, this.ravenStore);
        }
    }
}
