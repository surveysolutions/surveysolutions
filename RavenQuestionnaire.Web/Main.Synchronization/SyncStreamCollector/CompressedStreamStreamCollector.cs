// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompressedStreamStreamCollector.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The compressed stream stream collector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text;

namespace Main.Synchronization.SyncStreamCollector
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Events;
    using Main.Core.View.SyncProcess;

    using Newtonsoft.Json;

    using SynchronizationMessages.Export;

    using SynchronizationMessages.Export;

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

        public bool SupportSyncStat
        {
            get
            {
                return false;
            }
        }

        public List<UserSyncProcessStatistics> GetStat()
        {
            throw new NotImplementedException();
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
            if (zipStream == null)
            {
                var zipFile = new ZipFile(Encoding.UTF8);

                zipFile.CompressionLevel = CompressionLevel.BestSpeed;
                zipFile.ParallelDeflateThreshold = -1;

                var result =
                    JsonConvert.SerializeObject(
                        new ZipFileData { ClientGuid = this.ProcessGuid, Events = this.eventStore },
                        Formatting.None,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });

                zipFile.AddEntry("backup.txt", result, Encoding.UTF8);

                zipStream = new MemoryStream();
                zipFile.Save(zipStream);
            }

            zipStream.Position = 0;
            return zipStream;
        }


        private MemoryStream zipStream;


        /// <summary>
        /// The prepare to collect.
        /// </summary>
        public void PrepareToCollect()
        {
        }

        #endregion
    }
}