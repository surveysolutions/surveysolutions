using System;
using AndroidX.Work;

namespace WB.UI.Shared.Enumerator.Services.Notifications
{
    public class EnumeratorWorkerManager : IEnumeratorWorkerManager
    {
        private const string WORKER_TAG = "Intervier";
        public void SetNotificationsWorker()
        {
            var workerRequest = GetRequest();

            //save UUID for later use

            WorkManager.Instance.Enqueue(workerRequest);
        }

        public void CancelNotificationsWorker()
        {
            var workerRequest = GetRequest();
            WorkManager.Instance.CancelWorkById(workerRequest.Id);
        }

        private PeriodicWorkRequest GetRequest()
        {
            PeriodicWorkRequest notificationsGenerationRequest =
                PeriodicWorkRequest.Builder.From<NotificationsGenerationWorker>(TimeSpan.FromMinutes(20))
                    .SetConstraints(Constraints.None)
                    .AddTag(WORKER_TAG)
                    .Build();

            return notificationsGenerationRequest;
        }
    }
}
