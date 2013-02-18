namespace DataEntryClient.SycProcess.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IUsbSyncProcess : ISyncProcess
    {
        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <returns>
        /// Zip file with events
        /// </returns>
        byte[] Export(string syncProcessDescription);

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
        void Import(List<string> fileData, string description);
    }
}