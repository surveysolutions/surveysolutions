using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.UI.Shared.Enumerator.Services
{
    [Service]
    public class SyncBgService : Service
    {
        private ServiceBinder<SyncBgService> binder;
        private Thread thread;
        private bool isSyncRunning;

        public void StartSync()
        {
            if (!this.isSyncRunning)
            {
                var synchronizationProcess = Mvx.Resolve<ISynchronizationProcess>();
                this.CurrentProgress = new SyncProgressDto(new Progress<SyncProgressInfo>(), new CancellationTokenSource());

                this.thread = new Thread(() =>
                {
                    try
                    {
                        synchronizationProcess.SyncronizeAsync(this.CurrentProgress.Progress, this.CurrentProgress.CancellationTokenSource.Token)
                                              .WaitAndUnwrapException(); // do not pass cancellationToken, since it will always throw operation cancelled here
                    }
                    catch (System.OperationCanceledException ec)
                    {
                        Mvx.Resolve<ILoggerProvider>().GetFor<SyncBgService>().Error(">!>Failed to synchronize (canceled)", ec);
                    }
                    catch (Exception e)
                    {
                        Mvx.Resolve<ILoggerProvider>().GetFor<SyncBgService>().Error(">!>Failed to synchronize", e);
                    }
                    finally
                    {
                        this.isSyncRunning = false;
                        this.CurrentProgress = null;
                    }
                });

                this.isSyncRunning = true;
                this.thread.Start();
            }
        }

        public SyncProgressDto CurrentProgress { get; private set; }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new ServiceBinder<SyncBgService>(this);
            return this.binder;
        }
    }
}
