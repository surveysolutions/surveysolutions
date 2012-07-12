using System.Collections.Generic;
using System.IO;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Json.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Services
{
    public class RavenFileStorageService : IFileStorageService
    {
        private IDocumentStore documentStore;

        public RavenFileStorageService(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }
        public void StoreFile(string filename, Stream bytes)
        {
            Attachment a = documentStore.DatabaseCommands.GetAttachment(filename);
            if (a == null)
                documentStore.DatabaseCommands.PutAttachment(filename, null, bytes, new RavenJObject {});
        }

        public byte[] RetrieveFile(string filename)
        {
            Attachment a = documentStore.DatabaseCommands.GetAttachment(IdUtil.CreateImageId(filename));
            
            var memoryStream = new MemoryStream();
            a.Data().CopyTo(memoryStream);
            return memoryStream.ToArray();
             
            //return a.Data;
        }

        //public List<RavenJObject> RetrieveEventDocuments()
        //{
        //    return documentStore.DatabaseCommands.Query("Raven/DocumentsByEntityName", new IndexQuery
        //                   {
        //                        Query = "Tag:EventDocuments"
        //                   }, null).Results;
        //}
        
        public void DeleteFile(string filename)
        {
            documentStore.DatabaseCommands.DeleteAttachment(filename, null);
        }
    }
}
