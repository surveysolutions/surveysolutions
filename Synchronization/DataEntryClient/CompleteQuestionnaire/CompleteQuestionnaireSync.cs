using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DataEntryClient.WcfInfrastructure;
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
        private IChanelFactoryWrapper chanelFactoryWrapper;
        public CompleteQuestionnaireSync(ICommandInvoker invoker, IViewRepository viewRepository, IChanelFactoryWrapper chanelFactoryWrapper)
        {
            this.invoker = invoker;
            this.viewRepository = viewRepository;
            this.chanelFactoryWrapper = chanelFactoryWrapper;
        }


        public void Execute()
        {
            Guid syncKey =
                this.viewRepository.Load<ClientSettingsInputModel, ClientSettingsView>(new ClientSettingsInputModel()).
                    PublicKey;
            Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);
            UploadEvents(syncKey, lastSyncEventGuid);

        }
        public Guid? GetLastSyncEventGuid(Guid clientKey)
        {
            Guid? result = null;
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>((client) => { result = client.Process(clientKey); });
            return result;
        }

        public void UploadEvents(Guid clientKey, Guid? lastSyncEvent)
        {
            this.chanelFactoryWrapper.Execute<ICompleteQuestionnaireService>(
                (client)=>
                    {
                        var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(lastSyncEvent));
                        foreach (var eventItem in events.Items)
                        {
                            var message = new EventSyncMessage
                            {
                                SynchronizationKey = clientKey,
                                CommandKey = eventItem.PublicKey,
                                Command = eventItem.Command
                            };

                            ErrorCodes returnCode = client.Process(message);
                        }
                    }
                );
        }

    }
}
