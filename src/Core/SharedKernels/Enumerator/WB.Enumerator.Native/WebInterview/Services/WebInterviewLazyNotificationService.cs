using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewLazyNotificationService : WebInterviewNotificationService
    {
        public WebInterviewLazyNotificationService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IWebInterviewInvoker webInterviewInvoker)
            : base(statefulInterviewRepository, questionnaireStorage, webInterviewInvoker)
        {
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
                try
                {
                    InScopeExecutor.Current.Execute(action);
                }
                catch (NotSupportedException)
                {
                    // read side may not be avaliable for now
                    Thread.Sleep(5000);
                }
                catch { /* nom nom nom */ }
            }
        }

        private void AddToQueue(Action<IServiceLocator> action)
        {
            deferQueue.Add(action);
        }

        public override void RefreshEntities(Guid interviewId, params Identity[] questions) => AddToQueue((serviceLocator) => serviceLocator.GetInstance<WebInterviewNotificationService>().RefreshEntities(interviewId, questions));
        public override void RefreshRemovedEntities(Guid interviewId, params Identity[] entities) => AddToQueue((serviceLocator) => serviceLocator.GetInstance<WebInterviewNotificationService>().RefreshRemovedEntities(interviewId, entities));
        public override void RefreshEntitiesWithFilteredOptions(Guid interviewId) => AddToQueue((serviceLocator) => serviceLocator.GetInstance<WebInterviewNotificationService>().RefreshEntitiesWithFilteredOptions(interviewId));
        public override void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities) => AddToQueue((serviceLocator) => serviceLocator.GetInstance<WebInterviewNotificationService>().RefreshLinkedToListQuestions(interviewId, identities));
        public override void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities) => AddToQueue((serviceLocator) => serviceLocator.GetInstance<WebInterviewNotificationService>().RefreshLinkedToRosterQuestions(interviewId, rosterIdentities));
        public override void RefreshCascadingOptions(Guid interviewId, Identity identity) => AddToQueue(s => s.GetInstance<WebInterviewNotificationService>().RefreshCascadingOptions(interviewId, identity));
    }
}
