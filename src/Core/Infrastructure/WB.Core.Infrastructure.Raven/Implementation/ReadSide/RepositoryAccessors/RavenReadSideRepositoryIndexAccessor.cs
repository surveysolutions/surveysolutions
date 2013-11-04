using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenReadSideRepositoryIndexAccessor : IReadSideRepositoryIndexAccessor
    {
        private const string Database = "Views";

        private readonly DocumentStore ravenStore;
        private readonly Assembly[] assembliesWithIndexes;
        private bool wereIndexesCreated = false;

        public RavenReadSideRepositoryIndexAccessor(DocumentStore ravenStore, Assembly[] assembliesWithIndexes)
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
            this.EnsureDatabaseExists();
            this.EnsureIndexesExist();

            return this.ravenStore.OpenSession(Database);
        }

        private void EnsureDatabaseExists()
        {
            this.ravenStore.DatabaseCommands.EnsureDatabaseExists(Database);
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
            var catalog = new CompositionContainer(new AssemblyCatalog(assembly));
            IndexCreation.CreateIndexes(catalog, this.ravenStore.DatabaseCommands.ForDatabase(Database), this.ravenStore.Conventions);
        }
    }
}
