using System;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
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
            this.bus = NcqrsEnvironment.Get<IEventBus>();
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
        }


        public InterviewViewModel Load(QuestionnaireScreenInput input)
        {
            InterviewViewModel result = null;

            result = this.documentStorage.GetById(input.QuestionnaireId);

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
                /*var item = syncCacher.LoadItem(publicKey);
                if (!string.IsNullOrWhiteSpace(item))
                    RestoreFromSyncPackage(item);
                else*/
                    this.RestoreFromEventStream(publicKey);
            }
        }


        /*private void RestoreFromSyncPackage(string item)
        {
            string content = PackageHelper.DecompressString(item);
            var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

            commandService.Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));
            syncCacher.DeleteItem(interview.Id);


            //CapiApplication.Kernel.Get<IChangeLogManipulator>().CreateOrReopenDraftRecord(interview.Id);
        }*/

        private void RestoreFromEventStream(Guid publicKey)
        {
            var eventsAfterSnapshot = this.eventStore.ReadFrom(publicKey, 0, long.MaxValue);
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
                //logger.Error("Event: " + JsonUtils.GetJsonData(committedEvent.Payload));
                throw;
            }
            
        }
    }
}
