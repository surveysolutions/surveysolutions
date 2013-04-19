// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsbSyncProcess.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;
using SynchronizationMessages.Export;

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DataEntryClient.SycProcess.Interfaces;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Events;

    using Newtonsoft.Json;

    using Ninject;


    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public class UsbSyncProcess : AbstractSyncProcess, IUsbSyncProcess
    {
        #region Constants and Fields

        /// <summary>
        /// Zip file
        /// </summary>
        private ZipFile zip;

        /// <summary>
        /// List of text file content
        /// </summary>
        private List<string> zipData;

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
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <returns>
        /// Zip file with events
        /// </returns>
        public new byte[] Export(string syncProcessDescription)
        {
            base.Export(syncProcessDescription);

            var outputStream = new MemoryStream();
            this.zip.Save(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        /// <summary>
        /// The import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        [Obsolete("Import(string) is deprecated, please use Import(ZipFile, string) instead.", true)]
        public new ErrorCodes Import(string syncProcessDescription)
        {
            return ErrorCodes.Fail;
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="fileData">
        /// The file Data.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        public void Import(List<string> fileData, string description)
        {
            this.zipData = fileData;
            base.Import(description);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event exporter
        /// </summary>
        protected override void ExportEvents()
        {
            var collector = new EventPipeCollector();
            this.ProcessEvents(collector);

            this.zip = new ZipFile(Encoding.UTF8);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            this.zip.CompressionLevel = CompressionLevel.BestCompression;
            this.zip.AddEntry(
                "backup.txt", 
                JsonConvert.SerializeObject(
                    new ZipFileData { ClientGuid = this.ProcessGuid, Events = collector.GetEventList() }, 
                    Formatting.Indented,
                    settings), Encoding.UTF8);
        }

        /// <summary>
        /// Get events
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        protected override IEnumerable<AggregateRootEvent> GetEventStream()
        {
            var events = new List<AggregateRootEvent>();
            foreach (var file in this.zipData)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                var result = JsonConvert.DeserializeObject<ZipFileData>(file, settings);
                events.AddRange(result.Events);
            }

            return events;
        }

        #endregion
    }
}