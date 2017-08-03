using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewLazyNotificationService : WebInterviewNotificationService
    {
        private IPlainTransactionManager transactionManager
            => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private ITransactionManager readTransactionManager
            => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

        public WebInterviewLazyNotificationService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            [Named("WebInterview")] IHubContext webInterviewHubContext)
            : base(statefulInterviewRepository, questionnaireStorage, webInterviewHubContext)
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
                    transactionManager.ExecuteInQueryTransaction(() =>
                        readTransactionManager.ExecuteInQueryTransaction(action));
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