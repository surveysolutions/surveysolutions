using System.Reflection;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenReadSideRepositoryCleaner : IRavenReadSideRepositoryCleaner
    {
        private readonly IDocumentStore ravenStore;
        private readonly Assembly[] assembliesWithIndexes;

        public RavenReadSideRepositoryCleaner(IDocumentStore ravenStore, Assembly[] assembliesWithIndexes)
        {
            this.ravenStore = ravenStore;
            this.assembliesWithIndexes = assembliesWithIndexes;
        }

        public void ReCreateViewDatabase()
        {
            string ravenDbName = ((DocumentStore)this.ravenStore).DefaultDatabase;
            this.ravenStore.DatabaseCommands.GlobalAdmin.DeleteDatabase(ravenDbName, hardDelete: true);
            this.ravenStore.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists(ravenDbName, ignoreFailures: false);
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
            foreach (Assembly assembly in this.assembliesWithIndexes)
            {
                IndexCreation.CreateIndexes(assembly, this.ravenStore);
            }
        }
    }
}
