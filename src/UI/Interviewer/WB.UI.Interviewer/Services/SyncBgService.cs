using System;
using System.Threading;
using Android.OS;
using Android.App;
using Android.Content;
using MvvmCross.Platform;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Interviewer.Services
{
    [Service]
    public class SyncBgService : Service
    {
        private SyncServiceBinder binder;
        private Thread thread;
        private bool isSyncRunning;

        public void StartSync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            if (!this.isSyncRunning)
            {
                var synchronizationProcess = Mvx.Resolve<ISynchronizationProcess>();
                this.thread = new Thread(() =>
                {
                    try
                    {
                        synchronizationProcess.SyncronizeAsync(progress, cancellationToken)
                                              .WaitAndUnwrapException(); // do not pass cancellationToken, since it will always throw operation cancelled here
                    }
                    catch (Exception e)
                    {
                        Mvx.Resolve<ILoggerProvider>().GetFor<SyncBgService>().Error(">!>Failed to synchronize", e);
                    }
                    finally
                    {
                        isSyncRunning = false;
                    }
                });

                this.isSyncRunning = true;
                this.thread.Start();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new SyncServiceBinder(this);
            return this.binder;
        }
    }
}