using System;
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
        public void StoreFile( FileDescription file)
        {
            Attachment a = documentStore.DatabaseCommands.GetAttachment(file.PublicKey.ToString());
            if (a == null)
            {
                using (MemoryStream theMemStream = new MemoryStream())
                {

                    theMemStream.Write(file.Content, 0, file.Content.Length);
                    documentStore.DatabaseCommands.PutAttachment(file.PublicKey.ToString(), null, theMemStream,
                                                                 new RavenJObject
                                                                     {
                                                                         {"PublicKey", file.PublicKey.ToString()},
                                                                         {"Description", file.Description},
                                                                         {"Height", file.Height},
                                                                         {"Title", file.Title},
                                                                         {"Width", file.Width}
                                                                     });
                }
            }
        }

        public FileDescription RetrieveFile(string filename)
        {
            FileDescription file = new FileDescription();
            Attachment a = documentStore.DatabaseCommands.GetAttachment(filename);
            
            var memoryStream = new MemoryStream();
            a.Data().CopyTo(memoryStream);


            file.Content = memoryStream.ToArray();
            file.PublicKey =Guid.Parse( a.Metadata["PublicKey"].Value<string>());
            file.Description = a.Metadata["Description"].Value<string>();
            file.Title = a.Metadata["Description"].Value<string>();
            file.Height = a.Metadata["Height"].Value<int>();
            file.Width = a.Metadata["Width"].Value<int>();
            return file;
             
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
