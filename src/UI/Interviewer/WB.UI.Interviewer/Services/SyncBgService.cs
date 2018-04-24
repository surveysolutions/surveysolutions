using System;
using System.Threading;
using Android.OS;
using Android.App;
using Android.Content;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

namespace WB.UI.Interviewer.Services
{
    [Service]
    public class SyncBgService : Service
    {
        private SyncServiceBinder binder;
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
                    var auditLogService = ServiceLocator.Current.GetInstance<IAuditLogService>();
                    try
                    {
                        auditLogService.Write(new SynchronizationStartedAuditLogEntity());
                        synchronizationProcess.SyncronizeAsync(this.CurrentProgress.Progress, this.CurrentProgress.CancellationTokenSource.Token)
                                              .WaitAndUnwrapException(); // do not pass cancellationToken, since it will always throw operation cancelled here
                        auditLogService.Write(new SynchronizationCompletedAuditLogEntity());
                    }
                    catch (System.OperationCanceledException ec)
                    {
                        Mvx.Resolve<ILoggerProvider>().GetFor<SyncBgService>().Error(">!>Failed to synchronize (canceled)", ec);
                        auditLogService.Write(new SynchronizationCanceledAuditLogEntity());
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
            this.binder = new SyncServiceBinder(this);
            return this.binder;
        }
    }
}
