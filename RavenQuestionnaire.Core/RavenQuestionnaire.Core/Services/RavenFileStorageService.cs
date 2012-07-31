using System.IO;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Json.Linq;
using RavenQuestionnaire.Core.Documents;

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
            Attachment a = documentStore.DatabaseCommands.GetAttachment(file.PublicKey);
            if (a == null)
            {
              
              /*  using (MemoryStream theMemStream = new MemoryStream())
                {

                    theMemStream.Write(file.Content, 0, file.Content.Length);*/
                documentStore.DatabaseCommands.PutAttachment(file.PublicKey, null, file.Content,
                                                                 new RavenJObject
                                                                     {
                                                                         {"PublicKey", file.PublicKey},
                                                                         {"Description", file.Description},
                                                                         {"Title", file.Title}
                                                                     });
                file.Content.Position = 0;
                var image = new KalikoImage(file.Content);
                int thumbWidth, thumbHeight;
                var thumbData = ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                documentStore.DatabaseCommands.PutAttachment(GetThumbName(file.PublicKey), null, thumbData,
                                                                 new RavenJObject
                                                                     {
                                                                         {"PublicKey", GetThumbName(file.PublicKey)}
                                                                     });
               // }
            }
        }
        private MemoryStream ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            var thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));

            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

            //    var thumbData = new byte[ms.Length];
            //    ms.Read(thumbData, 0, thumbData.Length);

            newHeight = thumb.Height;
            newWidth = thumb.Width;

            return ms;
        }
   /*     public void StoreImage(Stream image, string title, string description)
        {
            throw new NotImplementedException();
        }*/

        public FileDescription RetrieveFile(string filename)
        {
            FileDescription file = new FileDescription();
            Attachment a = documentStore.DatabaseCommands.GetAttachment(filename);
            
         /*   var memoryStream = new MemoryStream();
            a.Data().CopyTo(memoryStream);*/


            file.Content = a.Data();
            file.PublicKey = filename;
            file.Description = a.Metadata["Description"].Value<string>();
            file.Title = a.Metadata["Description"].Value<string>();
            return file;
             
            //return a.Data;
        }

        public FileDescription RetrieveThumb(string filename)
        {
            return RetrieveFile(GetThumbName(filename));
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
            documentStore.DatabaseCommands.DeleteAttachment(GetThumbName(filename), null);
        }
        private string GetThumbName(string fileName)
        {
            return string.Format("{0}_thumb", fileName);
        }
    }
}
