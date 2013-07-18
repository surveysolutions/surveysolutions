using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenReadSideRepositoryIndexAccessor : IReadSideRepositoryIndexAccessor
    {
        private readonly DocumentStore ravenStore;

        private const string Database = "Views";

        protected RavenReadSideRepositoryIndexAccessor(DocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        protected IDocumentSession OpenSession()
        {
            this.ravenStore.DatabaseCommands.EnsureDatabaseExists(Database);
            return this.ravenStore.OpenSession(Database);
        }

        public IQueryable<TResult> Query<TResult>(string indexName)
        {
            using (IDocumentSession session = this.OpenSession())
            {
                return session.Query<TResult>(indexName);
            }
        }

        public void RegisterIndexesFormAssembly(Assembly assembly)
        {
            var catalog = new CompositionContainer(new AssemblyCatalog(assembly));
            IndexCreation.CreateIndexes(catalog, ravenStore.DatabaseCommands.ForDatabase(Database), ravenStore.Conventions);
        }
    }
}
