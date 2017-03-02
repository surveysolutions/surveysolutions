using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.ServiceLocation;
using Ninject;
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
            Task.Factory.StartNew(this.ProcessActionsInBackground, TaskCreationOptions.LongRunning);
        }

        private readonly BlockingCollection<Action> deferQueue = new BlockingCollection<Action>();

        private void ProcessActionsInBackground()
        {
            foreach (var action in this.deferQueue.GetConsumingEnumerable())
            {
                try
                {
                    this.transactionManager.ExecuteInQueryTransaction(() =>
                        this.readTransactionManager.ExecuteInQueryTransaction(action));
                }
                catch { /* nom nom nom */ }
            }
        }

        private void AddToQueue(Action action)
        {
            this.deferQueue.Add(action);
        }

        public override void RefreshEntities(Guid interviewId, params Identity[] questions) => this.AddToQueue(() => base.RefreshEntities(interviewId, questions));
        public override void RefreshRemovedEntities(Guid interviewId, params Identity[] questions) => this.AddToQueue(() => base.RefreshRemovedEntities(interviewId, questions));
        public override void RefreshEntitiesWithFilteredOptions(Guid interviewId) => this.AddToQueue(() => base.RefreshEntitiesWithFilteredOptions(interviewId));
        public override void RefreshLinkedToListQuestions(Guid interviewId, Identity[] identities) => this.AddToQueue(() => base.RefreshLinkedToListQuestions(interviewId, identities));
        public override void RefreshLinkedToRosterQuestions(Guid interviewId, Identity[] rosterIdentities) => this.AddToQueue(() => base.RefreshLinkedToRosterQuestions(interviewId, rosterIdentities));
    }
}