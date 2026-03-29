using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.UI.Shared.Enumerator.Services
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
    public class MapDownloadBackgroundService : Service
    {
        private ServiceBinder<MapDownloadBackgroundService> binder;
        private Thread thread;
        private bool isSyncRunning;

        private const int NotificationId = 2;
        private const string ChannelId = "map_sync_channel_id";

        public void SyncMaps()
        {
            if (!this.isSyncRunning)
            {
                var synchronizationProcess = Mvx.IoCProvider!.Resolve<IMapSyncProvider>();
                this.CurrentProgress = new MapSyncProgressStatus(new Progress<SyncProgressInfo>(), new CancellationTokenSource());
                this.isSyncRunning = true;

                StartForegroundNotification();

                this.thread = new Thread(() =>
                {
                    try
                    {
                        synchronizationProcess.SynchronizeAsync(this.CurrentProgress.Progress,
                                this.CurrentProgress.CancellationTokenSource.Token)
                            .WaitAndUnwrapException();
                        // do not pass cancellationToken, since it will always throw operation cancelled here
                    }
                    catch (Exception e)
                    {
                        Mvx.IoCProvider!.Resolve<ILoggerProvider>()!.GetFor<MapDownloadBackgroundService>()
                            .Error(">!>Failed to sync maps", e);
                    }
                    finally
                    {
                        this.isSyncRunning = false;
                        this.CurrentProgress = null;
                        StopForeground(true);
                        StopSelf();
                    }
                });

                this.thread.Start();
            }
        }

        private void StartForegroundNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, "Map Synchronization", NotificationImportance.Low)
                {
                    Description = "Map download progress"
                };
                var manager = (NotificationManager)GetSystemService(NotificationService)!;
                manager.CreateNotificationChannel(channel);
            }

            var notification = new Notification.Builder(this, ChannelId)
                .SetContentTitle("Map Synchronization")
                .SetContentText("Downloading maps...")
                .SetSmallIcon(Resource.Drawable.dashboard_sync_icon)
                .SetOngoing(true)
                .Build();

            if (Build.VERSION.SdkInt < BuildVersionCodes.UpsideDownCake)
            {
                StartForeground(NotificationId, notification);
            }
            else
            {
                StartForeground(NotificationId, notification, Android.Content.PM.ForegroundService.TypeDataSync);
            }
        }

        public override void OnDestroy()
        {
            var progress = this.CurrentProgress;
            if (progress != null && !progress.CancellationTokenSource.IsCancellationRequested)
            {
                progress.CancellationTokenSource.Cancel();
            }

            base.OnDestroy();
        }

        public MapSyncProgressStatus CurrentProgress { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new ServiceBinder<MapDownloadBackgroundService>(this);
            return this.binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.NotSticky;
        }
    }
}
