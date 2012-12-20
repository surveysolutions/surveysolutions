namespace DataEntryClient.SycProcess.Interfaces
{
    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// Wireless Sync Process
    /// </summary>
    public interface IWirelessSyncProcess : ISyncProcess
    {
        /// <summary>
        /// The Import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <returns>
        /// Error codes
        /// </returns>
        ErrorCodes Import(string syncProcessDescription, string baseAdress);

        /// <summary>
        /// The export
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        /// <returns>
        /// Error Codes
        /// </returns>
        ErrorCodes Export(string syncProcessDescription, string baseAdress);
    }
}