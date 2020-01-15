using System;
using Android.Content;
using Android.Runtime;
using AndroidX.Work;
using Java.Lang;
using MvvmCross;
using NLog;
using Exception = System.Exception;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class NotificationsGenerationWorker : Worker
    {
        public NotificationsGenerationWorker(IntPtr javaReference, JniHandleOwnership transfer) 
            : base(javaReference, transfer)
        {
        }

        public NotificationsGenerationWorker(Context context, WorkerParameters workerParams) 
            : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Trace("DoWork called");

            try
            {
                var collector = Mvx.IoCProvider.Resolve<INotificationsCollector>();
                var publisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();

                var notificationsToSend = collector.CollectAllNotifications();

                var context = this.ApplicationContext;

                publisher.Init(context);
                publisher.Notify(context, notificationsToSend, true);
                
                logger.Trace("DoWork InvokeSuccess");
                return Result.InvokeSuccess();
            }
            catch (InterruptedException e)
            {
                logger.Debug(e);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            return Result.InvokeFailure();
        }
    }
}
