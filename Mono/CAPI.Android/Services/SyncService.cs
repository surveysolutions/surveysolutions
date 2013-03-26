// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncService.cs" company="">
//   
// </copyright>
// <summary>
//   The sync service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Android.App;
using Android.Content;
using Android.OS;

namespace CAPI.Android.Services
{
    /// <summary>
    /// The sync service.
    /// </summary>
    [Service]
    [IntentFilter(new String[] { "org.worldbank.capi.sync" })]
    public class SyncService : IntentService
    {
        #region Constants


        private IBinder binder;
        private bool? result;

        public const string SyncFinishededAction = "SyncFinisheded";

        public bool? GetResult()
        {
            return result;
        }

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
            binder = new SyncServiceBinder(this);
            return binder;
        }

        protected override void OnHandleIntent(Intent intent)
        {
           result = this.Sync();
           var stocksIntent = new Intent(SyncFinishededAction);

           this.SendOrderedBroadcast(stocksIntent, null);
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

       /* /// <summary>
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
        }*/


        private bool Sync()
        {

            /*//check network avalability
            //ConnectivityManager cm = (ConnectivityManager)GetSystemService(Context.ConnectivityService);


            Guid processKey = Guid.NewGuid();
            string remoteSyncNode = "http://217.12.197.135/DEV-Supervisor/";
            string syncMessage = "Remote sync.";
            try
            {
                var streamProvider = new RemoteServiceEventStreamProvider1(CapiApplication.Kernel, processKey, remoteSyncNode);
                var collector = new LocalStorageStreamCollector(CapiApplication.Kernel, processKey);

                var manager = new SyncManager(streamProvider, collector, processKey, syncMessage, null);
                manager.StartPush();
            }
            catch (Exception)
            {
                return false;
            }
            */
            return true;
        }


        #endregion
    }

    /// <summary>
    /// The sync service binder.
    /// </summary>
    public class SyncServiceBinder : Binder
    {
        private SyncService service;

        public SyncServiceBinder(SyncService service)
        {
            this.service = service;
        }

        public SyncService GetSyncService()
        {
            return service;
        }
    }
}