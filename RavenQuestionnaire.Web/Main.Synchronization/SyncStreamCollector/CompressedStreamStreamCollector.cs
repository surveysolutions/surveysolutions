// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompressedStreamStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The compressed stream stream collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Events;

    using Newtonsoft.Json;

    using Questionnaire.Core.Web.Export;

    /// <summary>
    /// The compressed stream stream collector.
    /// </summary>
    public class CompressedStreamStreamCollector : ISyncStreamCollector
    {
        #region Fields

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly List<AggregateRootEvent> eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedStreamStreamCollector"/> class.
        /// </summary>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        public CompressedStreamStreamCollector(Guid processGuid)
        {
            this.ProcessGuid = processGuid;
            this.eventStore = new List<AggregateRootEvent>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the max chunk size.
        /// </summary>
        public int MaxChunkSize
        {
            get
            {
                return 1024;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the process guid.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        protected Guid ProcessGuid { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The collect.
        /// </summary>
        /// <param name="elements">
        /// The elements.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Collect(IEnumerable<AggregateRootEvent> elements)
        {
            if (elements != null)
            {
                this.eventStore.AddRange(elements);
            }

            return true;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The finish.
        /// </summary>
        public void Finish()
        {
        }

        /// <summary>
        /// The get exported stream.
        /// </summary>
        /// <returns>
        /// The <see cref="MemoryStream"/>.
        /// </returns>
        public MemoryStream GetExportedStream()
        {
            var zip = new ZipFile();

            zip.CompressionLevel = CompressionLevel.BestSpeed;

            zip.AddEntry(
                "backup.txt", 
                JsonConvert.SerializeObject(
                    new ZipFileData { ClientGuid = this.ProcessGuid, Events = this.eventStore }, 
                    Formatting.Indented, 
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects }));

            var outputStream = new MemoryStream();
            zip.Save(outputStream);

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }

        /// <summary>
        /// The prepare to collect.
        /// </summary>
        public void PrepareToCollect()
        {
        }

        #endregion
    }
}