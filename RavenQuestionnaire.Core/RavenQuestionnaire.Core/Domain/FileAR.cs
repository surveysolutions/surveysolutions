using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.File;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Domain
{
    public class FileAR : AggregateRootMappedByConvention
    {
    //    private FileDescription innerDocument=new FileDescription();
        private IFileStorageService storage = NcqrsEnvironment.Get<IFileStorageService>();
        private const string thumbFormat = "{0}_thumb";
        public FileAR()
        {
        }
        public FileAR(Guid publicKey, string title, string description,  int originalWidth, 
            int originalHeight, int thumbWidth, int thumbHeight, string originalFile, string thumbFile)
            : base(publicKey)
        {
            ApplyEvent(new FileUploaded
            {
                PublicKey = publicKey,
                Title = title,
                Description = description,
                OriginalFile = originalFile,
                OriginalHeight = originalHeight,
                OriginalWidth = originalWidth,
                ThumbFile = thumbFile,
                ThumbHeight = thumbHeight,
                ThumbWidth = thumbWidth
            });
        }
        protected void OnFileUploaded(FileUploaded e)
        {
            using (var original = FromBase64(e.OriginalFile))
            {
                var originalFile = new FileDescription()
                {
                    PublicKey = e.PublicKey.ToString(),
                    Content = original,
                    Description = e.Description,
                    Height = e.OriginalHeight,
                    Title = e.Title,
                    Width = e.OriginalWidth
                };
                storage.StoreFile(originalFile);

            }
            using (var thumb = FromBase64(e.ThumbFile))
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
            }
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
            storage.DeleteFile(e.PublicKey.ToString());
            storage.DeleteFile(string.Format(thumbFormat, e.PublicKey));
        }

        protected MemoryStream FromBase64(string text)
        {
            byte[] raw = Convert.FromBase64String(text);
            return new MemoryStream(raw);
        }
    }
}
