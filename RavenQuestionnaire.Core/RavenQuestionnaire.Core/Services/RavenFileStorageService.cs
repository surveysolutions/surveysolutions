using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Json.Linq;

namespace RavenQuestionnaire.Core.Services
{
    public class RavenFileStorageService : IFileStorageService
    {
        private IDocumentStore documentStore;

        public RavenFileStorageService(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }
        public void StoreFile(string filename, byte[] bytes)
        {
            documentStore.DatabaseCommands.PutAttachment(filename, null, bytes, new RavenJObject{});
        }

        public Attachment RetrieveFile(string filename)
        {
            return documentStore.DatabaseCommands.GetAttachment(filename);
        }
    }
}
