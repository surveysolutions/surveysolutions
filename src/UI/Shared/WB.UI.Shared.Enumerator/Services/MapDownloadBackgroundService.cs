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
    [Service]
    public class MapDownloadBackgroundService : Service
    {
        private ServiceBinder<MapDownloadBackgroundService> binder;
        private Thread thread;
        private bool isSyncRunning;

        public void SyncMaps()
        {
            if (!this.isSyncRunning)
             {
                var synchronizationProcess = Mvx.Resolve<IMapSyncProvider>();
                this.CurrentProgress = new MapSyncProgressStatus(new Progress<SyncProgressInfo>(), new CancellationTokenSource());

                this.thread = new Thread(() =>
                {
                    if (!this.isSyncRunning)
                    {
                        this.isSyncRunning = true;
                        try
                        {
                            synchronizationProcess.SynchronizeAsync(this.CurrentProgress.Progress,
                                    this.CurrentProgress.CancellationTokenSource.Token)
                                .WaitAndUnwrapException();
                            // do not pass cancellationToken, since it will always throw operation cancelled here
                        }
                        catch (Exception e)
                        {
                            Mvx.Resolve<ILoggerProvider>().GetFor<MapDownloadBackgroundService>()
                                .Error(">!>Failed to sync maps", e);
                        }
                        finally
                        {
                            this.isSyncRunning = false;
                            this.CurrentProgress = null;
                        }
                    }
                });
                
                this.thread.Start();
            }
        }

        public MapSyncProgressStatus CurrentProgress { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new ServiceBinder<MapDownloadBackgroundService>(this);
            return this.binder;
        }
    }
}
