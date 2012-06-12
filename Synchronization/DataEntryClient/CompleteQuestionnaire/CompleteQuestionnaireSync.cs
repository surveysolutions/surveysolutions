using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DataEntryClient.WcfInfrastructure;
using Ninject;
using Raven.Client;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync
    {
      //  private IKernel kernel;
        private IViewRepository viewRepository;
        private IChanelFactoryWrapper chanelFactoryWrapper;
        private IClientSettingsProvider clientSettingsProvider;
        private Guid processGuid;
     
        private ICommandInvoker invoker;
      //  private ICommandInvokerAsync invokerAsync;
        public CompleteQuestionnaireSync(IKernel kernel, Guid processGuid)
        {
            this.invoker = kernel.Get<ICommandInvoker>();
       //     this.invokerAsync = kernel.Get<ICommandInvokerAsync>();
            this.viewRepository = kernel.Get<IViewRepository>();
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.clientSettingsProvider = kernel.Get<IClientSettingsProvider>();
            this.processGuid = processGuid;
        }

        public void Execute()
        {
            Guid syncKey = clientSettingsProvider.ClientSettings.PublicKey;
            Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);
            UploadEvents(syncKey, lastSyncEventGuid);

        }
        public Guid? GetLastSyncEventGuid(Guid clientKey)
        {
            Guid? result = null;
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>(
                (client) =>
                    {
                        result = client.Process(clientKey);
                    });
            return result;
        }

        public void UploadEvents(Guid clientKey, Guid? lastSyncEvent)
        {
            this.chanelFactoryWrapper.Execute<IEventDocumentSync>(
                (client)=>
                    {
                        var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(lastSyncEvent));

                        invoker.ExecuteInSingleScope(new PushEventsCommand(processGuid, events.Items.Select(i => new EventDocument(i.Command, i.PublicKey, clientKey)), null));
                        foreach (var eventItem in events.Items)
                        {

                            invoker.ExecuteInSingleScope(new ChangeEventStatusCommand(processGuid, eventItem.PublicKey, EventState.InProgress, null));
                            var message = new EventSyncMessage
                            {
                                SynchronizationKey = clientKey,
                                CommandKey = eventItem.PublicKey,
                                Command = eventItem.Command
                            };

                            ErrorCodes returnCode = client.Process(message);
                            invoker.ExecuteInSingleScope(new ChangeEventStatusCommand(processGuid, eventItem.PublicKey,
                                                                         returnCode == ErrorCodes.None
                                                                             ? EventState.Completed
                                                                             : EventState.Error, null));
                        }
                    }
                );
            invoker.ExecuteInSingleScope(new EndProcessComand(processGuid, null));
        }

    }
}
