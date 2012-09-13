// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestionnaireSync.cs" company="">
//   
// </copyright>
// <summary>
//   The CompleteQuestionnaireSync interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;

    /// <summary>
    /// The CompleteQuestionnaireSync interface.
    /// </summary>
    public interface ICompleteQuestionnaireSync
    {
        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        void Export(Guid syncKey);

        /// <summary>
        /// The get last sync event guid.
        /// </summary>
        /// <param name="clientKey">
        /// The client key.
        /// </param>
        /// <returns>
        /// The System.Nullable`1[T -&gt; System.Guid].
        /// </returns>
        Guid? GetLastSyncEventGuid(Guid clientKey);

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        void Import(Guid syncKey);

        /// <summary>
        /// The upload events.
        /// </summary>
        /// <param name="clientKey">
        /// The client key.
        /// </param>
        /// <param name="lastSyncEvent">
        /// The last sync event.
        /// </param>
        void UploadEvents(Guid clientKey, Guid? lastSyncEvent);

        #endregion
    }
}