// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsbSyncProcess.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
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

    using Ninject;

    using Questionnaire.Core.Web.Export;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public class UsbSyncProcess : AbstractSyncProcess
    {
        #region Constants and Fields

        /// <summary>
        /// Zip file
        /// </summary>
        private ZipFile zip;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbSyncProcess"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcess">
        /// The sync Process.
        /// </param>
        public UsbSyncProcess(IKernel kernel, Guid syncProcess)
            : base(kernel, syncProcess)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// Zip file with events
        /// </returns>
        public new byte[] Export(Guid syncKey)
        {
            base.Export(syncKey);

            var outputStream = new MemoryStream();
            this.zip.Save(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <param name="uploadFile">
        /// The upload File.
        /// </param>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        public void Import(Guid syncKey, ZipFile uploadFile)
        {
            this.zip = uploadFile;
            this.Import(syncKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event exporter
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        protected override void ExportEvents(Guid syncKey)
        {
            var collector = new EventPipeCollector();
            this.ProcessEvents(syncKey, collector);

            this.zip = new ZipFile();
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            this.zip.CompressionLevel = CompressionLevel.BestCompression;
            this.zip.AddEntry(
                "backup.txt", 
                JsonConvert.SerializeObject(
                    new ZipFileData { ClientGuid = syncKey, Events = collector.GetEventList() }, 
                    Formatting.Indented, 
                    settings));
        }

        /// <summary>
        /// Get events
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        protected override IEnumerable<AggregateRootEvent> GetEventStream()
        {
            using (var stream = new MemoryStream())
            {
                foreach (ZipEntry e in this.zip)
                {
                    e.Extract(stream);
                }

                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                var data = Encoding.Default.GetString(stream.ToArray());
                var result = JsonConvert.DeserializeObject<ZipFileData>(data, settings);
                return result.Events;
            }
        }

        #endregion
    }
}