// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StorageSyncManager.cs" company="">
//   
// </copyright>
// <summary>
//   The storage sync manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Synchronization.SyncManager
{
    using System;
    using System.IO;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Synchronization.SyncSreamProvider;

    using Newtonsoft.Json;

    using Questionnaire.Core.Web.Export;

    // using SynchronizationMessages.Export;
    

    /// <summary>
    /// The storage sync manager.
    /// </summary>
    public class StorageSyncManager : ISyncManager
    {
        private ISyncEventStreamProvider eventStreamProvider;

        private SyncManagerSettings managerSettings;

        private Guid ProcessGuid ;

        public StorageSyncManager(ISyncEventStreamProvider provider, Guid syncProcess, SyncManagerSettings settings)
        {
            this.eventStreamProvider = provider;
            this.managerSettings = settings;
            this.ProcessGuid = syncProcess;

        }

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is working.
        /// </summary>
        public bool IsWorking { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get current progress.
        /// </summary>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public int? GetCurrentProgress()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The start pull.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPull()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The start push.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartPush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The start sync.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StartSynchronization()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The stop sync.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void StopProcess()
        {
            throw new NotImplementedException();
        }

        #endregion


        public MemoryStream GetExportedStream()
        {
            var zip = new ZipFile();

            zip.CompressionLevel = CompressionLevel.BestSpeed;
            zip.AddEntry(
                "backup.txt",
                JsonConvert.SerializeObject(
                    new ZipFileData { ClientGuid = this.ProcessGuid, Events = this.eventStreamProvider.GetEventStream() },
                    Formatting.Indented,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects }));

            var outputStream = new MemoryStream();
            zip.Save(outputStream);

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }
    }

    public class SyncManagerSettings
    {

    }
}