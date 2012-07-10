using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using DataEntryClient.WcfInfrastructure;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using Raven.Client;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync
    {
      //  private IKernel kernel;
      //  private IViewRepository viewRepository;
        private IChanelFactoryWrapper chanelFactoryWrapper;
        private IClientSettingsProvider clientSettingsProvider;
        private IEventSync eventStore;
        private Guid processGuid;
        private string baseAdress;
        private ICommandService invoker;
      //  private ICommandInvokerAsync invokerAsync;
        public CompleteQuestionnaireSync(IKernel kernel, Guid processGuid, string baseAdress)
        {
          //  this.invoker = kernel.Get<ICommandInvoker>();
       //     this.invokerAsync = kernel.Get<ICommandInvokerAsync>();
        //    this.viewRepository = kernel.Get<IViewRepository>();
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.clientSettingsProvider = kernel.Get<IClientSettingsProvider>();
            this.eventStore = kernel.Get<IEventSync>();
            this.invoker = NcqrsEnvironment.Get<ICommandService>();
               this.processGuid = processGuid;
            this.baseAdress = baseAdress;
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
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>(this.baseAdress,
                (client) =>
                    {
                        result = client.Process(clientKey);
                    });
            return result;
        }

        public void UploadEvents(Guid clientKey, Guid? lastSyncEvent)
        {
            this.chanelFactoryWrapper.Execute<IEventPipe>(this.baseAdress,
                (client)=>
                    {
                        var events = this.eventStore.ReadEvents();
                        invoker.Execute(new PushEventsCommand(this.processGuid, events));
                        foreach (AggregateRootEventStream aggregateRootEventStream in events)
                        {
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                         aggregateRootEventStream.SourceId,
                                                                         EventState.InProgress));
                            var message = new EventSyncMessage
                                              {
                                                  Command = aggregateRootEventStream,
                                                  SynchronizationKey = clientKey,
                                                  CommandKey = aggregateRootEventStream.SourceId
                                              };
                            ErrorCodes returnCode = client.Process(message);
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                       aggregateRootEventStream.SourceId,
                                                                       returnCode == ErrorCodes.None
                                                                             ? EventState.Completed
                                                                             : EventState.Error));

                        }
                        invoker.Execute(new EndProcessComand(this.processGuid));
                        /*  var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(lastSyncEvent));
                        var eventList =
                            events.Items.Select(i => new EventDocument(i.Command, i.PublicKey, clientKey)).ToList();
                        invoker.ExecuteInSingleScope(new PushEventsCommand(processGuid, eventList, null));*/
                        /*   foreach (var eventItem in events.Items)
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
                        }*/
                    }
                );
         //   invoker.ExecuteInSingleScope(new EndProcessComand(processGuid, null));
        }

    }
}
