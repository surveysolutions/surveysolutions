// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISyncProcess.cs" company="">
//   
// </copyright>
// <summary>
//   The WirelessSyncProcess interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// The WirelessSyncProcess interface.
    /// </summary>
    public interface ISyncProcess
    {
        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        ErrorCodes Export(string description);

        /// <summary>
        /// The import.
        /// </summary>
        ErrorCodes Import(string description);

        #endregion
    }
}