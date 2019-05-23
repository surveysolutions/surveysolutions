using System;
using AndroidX.Work;
using Java.Util.Concurrent;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class EnumeratorWorkerManager : IEnumeratorWorkerManager
    {
        private const string WORKER_TAG = "Interviewer";
        public void SetNotificationsWorker()
        {
            PeriodicWorkRequest notificationsGenerationRequest =
                PeriodicWorkRequest.Builder.From<NotificationsGenerationWorker>(60, TimeUnit.Minutes)
                    .SetConstraints(Constraints.None)
                    .AddTag(WORKER_TAG)
                    .Build();

            WorkManager.Instance.EnqueueUniquePeriodicWork(WORKER_TAG, 
                ExistingPeriodicWorkPolicy.Keep,
                notificationsGenerationRequest);
        }

        public void CancelNotificationsWorker()
        {
            WorkManager.Instance.CancelUniqueWork(WORKER_TAG);
        }
    }
}
