// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RavenFileStorageService.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The raven file storage service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;
using Main.Core.Documents;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Main.Core.Services
{
    /// <summary>
    /// The raven file storage service.
    /// </summary>
    public class RavenFileStorageService : IFileStorageService
    {
        #region Fields

        /// <summary>
        /// The document store.
        /// </summary>
        private readonly IDocumentStore documentStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenFileStorageService"/> class.
        /// </summary>
        /// <param name="documentStore">
        /// The document store.
        /// </param>
        public RavenFileStorageService(DocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The delete file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        public void DeleteFile(string filename)
        {
            this.documentStore.DatabaseCommands.DeleteAttachment(filename, null);
            this.documentStore.DatabaseCommands.DeleteAttachment(this.GetThumbName(filename), null);
        }

        /*     public void StoreImage(Stream image, string title, string description)
        {
            throw new NotImplementedException();
        }*/

        /// <summary>
        /// The retrieve file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.FileDescription.
        /// </returns>
        public FileDescription RetrieveFile(string filename)
        {
            var file = new FileDescription();
            Attachment a = this.documentStore.DatabaseCommands.GetAttachment(filename);

            /*   var memoryStream = new MemoryStream();
            a.Data().CopyTo(memoryStream);*/

            file.Content = a.Data();

            file.FileName = filename;

            file.Description = a.Metadata["Description"].Value<string>();
            file.Title = a.Metadata["Description"].Value<string>();
            return file;

            // return a.Data;
        }

        /// <summary>
        /// The retrieve thumb.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.FileDescription.
        /// </returns>
        public FileDescription RetrieveThumb(string filename)
        {
            return this.RetrieveFile(this.GetThumbName(filename));
        }

        /// <summary>
        /// The store file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        public void StoreFile(FileDescription file)
        {
            Attachment a = this.documentStore.DatabaseCommands.GetAttachment(file.FileName);
            if (a == null)
            {
                /*  using (MemoryStream theMemStream = new MemoryStream())
                {

                    theMemStream.Write(file.Content, 0, file.Content.Length);*/
                this.documentStore.DatabaseCommands.PutAttachment(
                    file.FileName, 
                    null, 
                    file.Content, 
                    new RavenJObject
                        {
                            { "PublicKey", file.FileName}, 
                            { "Description", file.Description }, 
                            { "Title", file.Title }
                        });
                file.Content.Position = 0;
                KalikoImage image = new KalikoImage(file.Content);
                int thumbWidth, thumbHeight;
                MemoryStream thumbData = this.ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                this.documentStore.DatabaseCommands.PutAttachment(
                    this.GetThumbName(file.FileName), 
                    null, 
                    thumbData, 
                    new RavenJObject { { "PublicKey", this.GetThumbName(file.FileName) } });

                // }
            }
        }

        #endregion

        // public List<RavenJObject> RetrieveEventDocuments()
        // {
        // return documentStore.DatabaseCommands.Query("Raven/DocumentsByEntityName", new IndexQuery
        // {
        // Query = "Tag:EventDocuments"
        // }, null).Results;
        // }
        #region Methods

        /// <summary>
        /// The get thumb name.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private string GetThumbName(string fileName)
        {
            return string.Format("{0}_thumb", fileName);
        }

        /// <summary>
        /// The resize image.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="newWidth">
        /// The new width.
        /// </param>
        /// <param name="newHeight">
        /// The new height.
        /// </param>
        /// <returns>
        /// The System.IO.MemoryStream.
        /// </returns>
        private MemoryStream ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            KalikoImage thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));

            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

            // var thumbData = new byte[ms.Length];
            // ms.Read(thumbData, 0, thumbData.Length);
            newHeight = thumb.Height;
            newWidth = thumb.Width;

            return ms;
        }

        #endregion
    }
}