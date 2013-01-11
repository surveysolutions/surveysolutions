// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleSyncService.cs" company="">
//   
// </copyright>
// <summary>
//   The simple sync service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidApp.Services
{
    using System;

    using Android.App;
    using Android.Content;

    /// <summary>
    /// The simple sync service.
    /// </summary>
    [Service]
    public class SimpleSyncService : IntentService
    {
        public const string SyncStartedAction = "SyncStarted";

        public const string SyncFinishedAction = "SyncFinished";

        #region Methods

        /// <summary>
        /// The on handle intent.
        /// </summary>
        /// <param name="intent">
        /// The intent.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        protected override void OnHandleIntent(Intent intent)
        {
            throw new NotImplementedException();
            string TargetUrl = "";
        }





        #endregion
    }
}