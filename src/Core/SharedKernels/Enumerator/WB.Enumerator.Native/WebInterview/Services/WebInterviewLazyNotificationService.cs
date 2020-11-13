using System;
using System.Collections.Concurrent;
using System.Threading;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewLazyNotificationService : WebInterviewNotificationService
    {
        private readonly IAggregateRootCache aggregateRootCache;

        public WebInterviewLazyNotificationService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IWebInterviewInvoker webInterviewInvoker,
            IAggregateRootCache aggregateRootCache)
            : base(statefulInterviewRepository, questionnaireStorage, webInterviewInvoker)
        {
            this.aggregateRootCache = aggregateRootCache;
        }

        static WebInterviewLazyNotificationService()
        {
            // can be safely added more tasks if for some reason we are not notifying users in-time
            ExecutionTask = new Thread(ProcessActionsInBackground);
            ExecutionTask.Start();
        }

        private static Thread ExecutionTask { get; }

        private static readonly BlockingCollection<Action<IServiceLocator>> deferQueue = new BlockingCollection<Action<IServiceLocator>>();

        private static void ProcessActionsInBackground()
        {
            foreach (var action in deferQueue.GetConsumingEnumerable())
            {
                CommonMetrics.WebInterviewNotifications.Dec();
                try
                {
                    InScopeExecutor.Current.Execute(action);
                }
                catch (NotSupportedException)
                {
                    // read side may not be available for now
                    Thread.Sleep(5000);
                }
                catch { /* nom nom nom */ }
            }
        }

        private void AddToQueue(Guid interviewId, Action<IServiceLocator> action)
        {
            if (aggregateRootCache.GetConnectedCount(interviewId) > 0)
            {
                CommonMetrics.WebInterviewNotifications.Inc();
                deferQueue.Add(action);
            }
        }

        private void AddToQueue(Guid interviewId, Action<WebInterviewNotificationService> action)
        {
            AddToQueue(interviewId, sl => action(sl.GetInstance<WebInterviewNotificationService>()));
        }

        public override void RefreshEntities(Guid interviewId, params Identity[] questions) =>
            AddToQueue(interviewId, s => s.RefreshEntities(interviewId, questions));

        public override void RefreshRemovedEntities(Guid interviewId, params Identity[] entities) =>
            AddToQueue(interviewId, s => s.RefreshRemovedEntities(interviewId, entities));
    }
}
