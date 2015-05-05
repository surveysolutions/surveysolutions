using System;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, InterviewViewModel>
    {
        private readonly IReadSideRepositoryReader<InterviewViewModel> documentStorage;
        private readonly IEventStore eventStore;
        private readonly IEventBus bus;
        private readonly ILogger logger;
        
        private object syncLock = new object();

        public QuestionnaireScreenViewFactory(IReadSideRepositoryReader<InterviewViewModel> documentStorage)
        {
            this.documentStorage = documentStorage;
            this.bus = ServiceLocator.Current.GetInstance<IEventBus>("interviewViewBus"); //NcqrsEnvironment.Get<IEventBus>();
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
        }


        public InterviewViewModel Load(QuestionnaireScreenInput input)
        {
            InterviewViewModel result = this.documentStorage.GetById(input.QuestionnaireId);

            if (result != null)
                return result;
            
            this.PublishEventsToFillMemoryStorage(input.QuestionnaireId);

            result = this.documentStorage.GetById(input.QuestionnaireId);
            
            return result;
        }

        private void PublishEventsToFillMemoryStorage(Guid publicKey)
        {
            lock (this.syncLock)
            {
                this.RestoreFromEventStream(publicKey);
            }
        }

        private void RestoreFromEventStream(Guid publicKey)
        {
            var eventsAfterSnapshot = this.eventStore.ReadFrom(publicKey, 0, int.MaxValue);
            try
            {
                foreach (CommittedEvent committedEvent in eventsAfterSnapshot)
                {
                    this.bus.Publish(committedEvent);
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Rebuild Error", e);
                throw;
            }
            
        }
    }
}
