using System;
using System.Threading;
using Android.OS;
using Android.App;
using Android.Content;
using MvvmCross;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.UI.Interviewer.Services
{
    [Service]
    public class MapDownloadBackgroundService : Service
    {
        private MapDownloadBackgroundServiceBinder binder;
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
                            synchronizationProcess.SyncronizeAsync(this.CurrentProgress.Progress,
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
            this.binder = new MapDownloadBackgroundServiceBinder(this);
            return this.binder;
        }
    }
}
