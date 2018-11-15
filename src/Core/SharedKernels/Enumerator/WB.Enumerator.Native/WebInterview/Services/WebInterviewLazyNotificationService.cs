using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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
            // can be safely added more tasks if for some reason we are not notify users in-time
            Task.Factory.StartNew(ProcessActionsInBackground, TaskCreationOptions.LongRunning);
        }

        private readonly BlockingCollection<Action> deferQueue = new BlockingCollection<Action>();

        private void ProcessActionsInBackground()
        {
            foreach (var action in deferQueue.GetConsumingEnumerable())
            {
                try
                {
                    action();
                }
                catch (NotSupportedException)
                {
                    // read side may not be avaliable for now
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                }
                catch { /* nom nom nom */ }
            }
        }

        private void AddToQueue(Action action)
        {
            deferQueue.Add(action);
        }

        public override void RefreshEntities(Guid interviewId, params Identity[] questions) => AddToQueue(() => base.RefreshEntities(interviewId, questions));
        public override void RefreshRemovedEntities(Guid interviewId, params Identity[] entities) => AddToQueue(() => base.RefreshRemovedEntities(interviewId, entities));
        public override void RefreshEntitiesWithFilteredOptions(Guid interviewId) => AddToQueue(() => base.RefreshEntitiesWithFilteredOptions(interviewId));
        public override void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities) => AddToQueue(() => base.RefreshLinkedToListQuestions(interviewId, identities));
        public override void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities) => AddToQueue(() => base.RefreshLinkedToRosterQuestions(interviewId, rosterIdentities));
    }
}
