using System;
using System.Threading;
using Android.OS;
using Android.App;
using Android.Content;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Services;

namespace WB.UI.Interviewer.Services
{
    [Service]
    public class SyncBgService : Service
    {
        private SyncServiceBinder binder;
        private Thread thread;

        public void StartSync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            this.thread = new Thread(() =>
            {
                 Mvx.Resolve<ISynchronizationProcess>().SyncronizeAsync(progress, cancellationToken).WaitAndUnwrapException(cancellationToken);
            });
            this.thread.Start();
        }

        public override IBinder OnBind(Intent intent)
        {
            this.binder = new SyncServiceBinder(this);
            return this.binder;
        }
    }
}