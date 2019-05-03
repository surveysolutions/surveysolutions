using System;
using Android.Content;
using Android.Runtime;
using AndroidX.Work;
using Java.Lang;
using MvvmCross;
using Exception = System.Exception;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class NotificationsGenerationWorker : Worker
    {
        public NotificationsGenerationWorker(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public NotificationsGenerationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
        {
        }

        public override Result DoWork()
        {
            Android.Util.Log.Debug("NotificationsGenerationWorker", $"Was called");

            try
            {
                var collector = Mvx.IoCProvider.Resolve<INotificationsCollector>();
                var publisher = Mvx.IoCProvider.Resolve<INotificationPublisher>();

                var notificationsToSend = collector.CollectAllNotifications();

                var context = this.ApplicationContext;

                publisher.Init(context);

                foreach (var notificationModel in notificationsToSend)
                {
                    publisher.Notify(context, notificationModel);
                }

                return Result.InvokeSuccess();
            }
            catch (InterruptedException e)
            {
                Android.Util.Log.Debug("NotificationsGenerationWorker", $"Error (InterruptedException): {e.Message}");

                return Result.InvokeFailure();
            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("NotificationsGenerationWorker", $"Error: {e.Message}");
                return Result.InvokeFailure();
            }
        }
    }
}
