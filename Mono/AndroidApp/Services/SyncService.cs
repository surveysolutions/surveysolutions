// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncService.cs" company="">
//   
// </copyright>
// <summary>
//   The sync service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AndroidApp.Services
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Util;

    /// <summary>
    /// The sync service.
    /// </summary>
    [Service]
    public class SyncService : IntentService
    {
        #region Constants

        /// <summary>
        /// The sync command.
        /// </summary>
        public const string SyncCommand = "RunSync";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on bind.
        /// </summary>
        /// <param name="intent">
        /// The intent.
        /// </param>
        /// <returns>
        /// The <see cref="IBinder"/>.
        /// </returns>
        public override IBinder OnBind(Intent intent)
        {
            // throw new NotImplementedException();
            return null;
        }

        protected override void OnHandleIntent(Intent intent)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// The on create.
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();
        }

        /// <summary>
        /// The on destroy.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// The on start.
        /// </summary>
        /// <param name="intent">
        /// The intent.
        /// </param>
        /// <param name="startId">
        /// The start id.
        /// </param>
        public override void OnStart(Intent intent, int startId)
        {
            base.OnStart(intent, startId);
        }

        /// <summary>
        /// The on start command.
        /// </summary>
        /// <param name="intent">
        /// The intent.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="startId">
        /// The start id.
        /// </param>
        /// <returns>
        /// The <see cref="StartCommandResult"/>.
        /// </returns>
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug("Sync", "Sync started");

            return StartCommandResult.NotSticky;
        }


        #endregion
    }
}