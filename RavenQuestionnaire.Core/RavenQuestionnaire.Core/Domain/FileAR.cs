using System;
using System.IO;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.File;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Domain
{
    public class FileAR : AggregateRootMappedByConvention
    {
        private FileDescription innerDocument=new FileDescription();
        private string base64Content = string.Empty;
    //    private IFileStorageService storage = NcqrsEnvironment.Get<IFileStorageService>();
       
        public FileAR()
        {
        }
        public FileAR(Guid publicKey, string title, string description, string originalFile)
            : base(publicKey)
        {
            ApplyEvent(new FileUploaded
            {
                PublicKey = publicKey,
                Title = title,
                Description = description,
                OriginalFile = originalFile
            });
        }
        protected void OnFileUploaded(FileUploaded e)
        {

            innerDocument = new FileDescription()
                                {
                                    PublicKey = e.PublicKey.ToString(),
                                    Description = e.Description,
                                    Title = e.Title
                                };
            base64Content = e.OriginalFile;
            // storage.StoreFile(originalFile);


            /*    using (var thumb = FromBase64(e.ThumbFile))
                {
                    var tumbFile = new FileDescription()
                    {
                        PublicKey =string.Format(thumbFormat,e.PublicKey),
                        Content = thumb,
                        Title = string.Empty,
                        Description = string.Empty
                    };
                    storage.StoreFile(tumbFile);
                  //  attachments.Store(tumbFile, evnt.Payload.ImagePublicKey);
                }*/
        }

        public void UpdateFileMeta(string title, string description)
        {
            ApplyEvent(new FileMetaUpdated() {PublicKey = EventSourceId, Description = description, Title = title});
        }
        protected void OnFileMetaUpdated(FileMetaUpdated e)
        {
        
        }
        public void DeleteFile()
        {
            ApplyEvent(new FileDeleted() {PublicKey = EventSourceId});
        }
        public void OnFileDeleted(FileDeleted e)
        {
            innerDocument = null;
            base64Content = string.Empty;
            //  storage.DeleteFile(e.PublicKey.ToString());
            //   storage.DeleteFile(string.Format(thumbFormat, e.PublicKey));
        }
       
       
    }
}
