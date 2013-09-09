using System;
using System.Threading;
using CAPI.Android.Core.Model.ModelUtils;
using CAPI.Android.Core.Model.SyncCacher;
using Main.Core;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, CompleteQuestionnaireView>
    {
        private readonly IReadSideRepositoryReader<CompleteQuestionnaireView> documentStorage;
        private readonly IEventStore eventStore;
        private readonly IEventBus bus;
        private readonly ISyncCacher syncCacher;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private bool isWorking = false;

        public QuestionnaireScreenViewFactory(IReadSideRepositoryReader<CompleteQuestionnaireView> documentStorage, ISyncCacher syncCacher)
        {
            this.documentStorage = documentStorage;
            this.syncCacher = syncCacher;

            this.bus = NcqrsEnvironment.Get<IEventBus>();
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
        }


        public CompleteQuestionnaireView Load(QuestionnaireScreenInput input)
        {
            CompleteQuestionnaireView result = null;

            result = this.documentStorage.GetById(input.QuestionnaireId);

            if (result != null)
                return result;
            
            GenerateEvents(input.QuestionnaireId);
            result = this.documentStorage.GetById(input.QuestionnaireId);

            return result;
        }

        private void GenerateEvents(Guid publicKey)
        {
            if (isWorking)
            {
                WaitForRestoreFromOtherThread();
                return;
            }

            isWorking = true;

            var item = syncCacher.LoadItem(publicKey);
            if (!string.IsNullOrWhiteSpace(item))
                RestoreFromSyncPackage(item);
            else
                RestoreFromEventStream(publicKey);

            isWorking = false;
        }

        private void WaitForRestoreFromOtherThread()
        {
            int i = 0;
            while (isWorking && i < 60)
            {
                Thread.Sleep(1000);
                i++;
            }
        }

        private void RestoreFromSyncPackage(string item)
        {
            string content = PackageHelper.DecompressString(item);
            var interview = JsonUtils.GetObject<InterviewSynchronizationDto>(content);

            commandService.Execute(new SynchronizeInterviewCommand(interview.Id, interview.UserId, interview));

            syncCacher.DeleteItem(interview.Id);
        }

        private void RestoreFromEventStream(Guid publicKey)
        {
            var eventsAfterSnapshot = eventStore.ReadFrom(publicKey, 0, long.MaxValue);
            foreach (CommittedEvent committedEvent in eventsAfterSnapshot)
            {
                try
                {
                    bus.Publish(committedEvent);
                }
                catch (Exception e)
                {
                    logger.Error("Rebuild Error", e);
                    logger.Error("Event: " + JsonUtils.GetJsonData(committedEvent.Payload));
                }
            }
        }
    }
}
