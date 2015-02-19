using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenReadSideRepositoryCleaner : IRavenReadSideRepositoryCleaner
    {
        private readonly IDocumentStore ravenStore;

        public RavenReadSideRepositoryCleaner(IDocumentStore ravenStore)
        {
            this.ravenStore = ravenStore;
        }

        public void ReCreateViewDatabase()
        {
            string ravenDbName = ((DocumentStore)this.ravenStore).DefaultDatabase;
            this.ravenStore.DatabaseCommands.GlobalAdmin.DeleteDatabase(ravenDbName, hardDelete: true);
            this.ravenStore.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists(ravenDbName, ignoreFailures: false);
        }
    }
}
