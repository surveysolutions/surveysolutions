// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportImportEvent.cs" company="">
//   
// </copyright>
// <summary>
//   The i export import.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Questionnaire.Core.Web.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Events;

    using Newtonsoft.Json;

    /// <summary>
    /// The i export import.
    /// </summary>
    public interface IExportImport
    {
        #region Public Methods and Operators

        /// <summary>
        /// Export events for given user
        /// </summary>
        /// <param name="clientGuid">
        /// Client guid.
        /// </param>
        /// <returns>
        /// Zip archive contains backup.txt file with serialized events for given user
        /// </returns>
        byte[] Export(Guid clientGuid);

        /// <summary>
        /// Import data from *.capi file
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        void Import(HttpPostedFileBase uploadFile);

        #endregion
    }

    /// <summary>
    /// The export import event helper class.
    /// </summary>
    public class ExportImportEvent : IExportImport
    {
        #region Fields

        /// <summary>
        /// The synchronizer.
        /// </summary>
        private readonly IEventSync synchronizer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportImportEvent"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer to collect events
        /// </param>
        public ExportImportEvent(IEventSync synchronizer)
        {
            this.synchronizer = synchronizer;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Export events for given user
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// Zip archive contains backup.txt file with serialized events for given user
        /// </returns>
        public byte[] Export(Guid clientGuid)
        {
            return this.ExportInternal(clientGuid, this.synchronizer.ReadEvents(), "backup.txt");
        }


        /// <summary>
        /// Import data from *.capi file
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        public void Import(HttpPostedFileBase uploadFile)
        {
            if (ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                uploadFile.InputStream.Position = 0;

                ZipFile zip = ZipFile.Read(uploadFile.InputStream);
                using (var stream = new MemoryStream())
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(stream);
                    }

                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                    string data = Encoding.Default.GetString(stream.ToArray());
                    var result = JsonConvert.DeserializeObject<ZipFileData>(
                        data, settings);

                    this.synchronizer.WriteEvents(result.Events);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// Zip file as array of bytes
        /// </returns>
        protected byte[] ExportInternal(Guid? clientGuid, IEnumerable<AggregateRootEvent> events, string fileName)
        {
            var data = new ZipFileData { ClientGuid = clientGuid == null ? Guid.Empty : clientGuid.Value, Events = events };
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.None;
                zip.AddEntry(fileName, JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }


        #endregion
    }
}