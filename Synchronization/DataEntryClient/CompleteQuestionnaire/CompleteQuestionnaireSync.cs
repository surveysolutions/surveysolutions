using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync
    {
        private ICommandInvoker invoker;
        private IViewRepository viewRepository;
        public CompleteQuestionnaireSync(ICommandInvoker invoker, IViewRepository viewRepository)
        {
            this.invoker = invoker;
            this.viewRepository = viewRepository;
        }


        public void Execute()
        {

            Guid syncKey =
                this.viewRepository.Load<ClientSettingsInputModel, ClientSettingsView>(new ClientSettingsInputModel()).
                    PublicKey;
            Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);
            UploadEvents(syncKey, lastSyncEventGuid);

        }
        protected Guid? GetLastSyncEventGuid(Guid clientKey)
        {
            Guid? result=null;
            ChannelFactory<IGetLastSyncEvent> ChannelFactory = new ChannelFactory<IGetLastSyncEvent>("");
            IGetLastSyncEvent client = ChannelFactory.CreateChannel();
            try
            {

                result = client.Process(clientKey);
            }
            finally
            {
                try
                {
                    ((IChannel)client).Close();
                }
                catch
                {
                    ((IChannel)client).Abort();
                }
            }
            return result;
        }

        protected void UploadEvents(Guid clientKey, Guid? lastSyncEvent)
        {
            ChannelFactory<ICompleteQuestionnaireService> ChannelFactory = new ChannelFactory<ICompleteQuestionnaireService>("");
            ICompleteQuestionnaireService client = ChannelFactory.CreateChannel();
            try
            {

                var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(lastSyncEvent));
                foreach (var eventItem in events.Items)
                {
                    var message = new EventSyncMessage
                    {
                        SynchronizationKey = clientKey,
                        Command = eventItem.Command
                    };

                    ErrorCodes returnCode = client.Process(message);
                }
            }
            finally
            {
                try
                {
                    ((IChannel)client).Close();
                }
                catch
                {
                    ((IChannel)client).Abort();
                }
            }
        }
    }
}
