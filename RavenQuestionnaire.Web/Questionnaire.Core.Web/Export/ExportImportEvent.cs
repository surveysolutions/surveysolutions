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
    using System.Linq;
    using System.Text;
    using System.Web;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs.Restoring.EventStapshoot;

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

        /// <summary>
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        byte[] ExportTemplate(Guid? id, Guid clientGuid);

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
            return this.ExportInternal(clientGuid, this.synchronizer.ReadEvents);
        }

        /// <summary>
        /// Export template
        /// </summary>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <returns>
        /// Zip archive contains all event connected with template questionnaire
        /// </returns>
        public byte[] ExportTemplate(Guid? templateGuid, Guid clientGuid)
        {
            return this.ExportTemplateInternal(templateGuid, clientGuid);
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
        protected byte[] ExportInternal(Guid clientGuid, Func<IEnumerable<AggregateRootEvent>> action)
        {
            var data = new ZipFileData { ClientGuid = clientGuid, Events = action() };
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.None;
                zip.AddEntry("backup.txt", JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        protected byte[] ExportTemplateInternal(Guid? templateGuid, Guid clientGuid)
        {
            var archive = new List<AggregateRootEvent>();
            var events = this.synchronizer.ReadEvents().ToList();
            if (templateGuid != null)
                archive.Add(events.Where(t =>
                    {
                        var payload = ((t.Payload as SnapshootLoaded).Template).Payload;
                        return payload != null && (payload as QuestionnaireDocument).PublicKey == templateGuid;
                    }).FirstOrDefault());
            else archive = events;
            var data = new ZipFileData { ClientGuid = clientGuid, Events = archive };
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.None;
                string name = string.Format("template{0}.txt", templateGuid == null ? "s" : string.Empty);
                zip.AddEntry(name, JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        #endregion
    }
}