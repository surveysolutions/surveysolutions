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
    public class CompleteQuestionnaireSync : ICompleteQuestionnaireSync
    {
        private IChanelFactoryWrapper chanelFactoryWrapper;
        private IClientSettingsProvider clientSettingsProvider;
        private IEventSync eventStore;
        private Guid processGuid;
        private string baseAdress;
        private ICommandService invoker;
        public CompleteQuestionnaireSync(IKernel kernel, Guid processGuid, string baseAdress)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
            this.clientSettingsProvider = kernel.Get<IClientSettingsProvider>();
            this.eventStore = kernel.Get<IEventSync>();
            this.invoker = NcqrsEnvironment.Get<ICommandService>();
            this.processGuid = processGuid;
            this.baseAdress = baseAdress;
        }

        public void Export()
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
                        var events = this.eventStore.ReadCompleteQuestionare();
                        invoker.Execute(new PushEventsCommand(this.processGuid, events));
                        foreach (AggregateRootEventStream aggregateRootEventStream in events)
                        {
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                         aggregateRootEventStream.SourceId,
                                                                         EventState.InProgress));
                            var message = new EventSyncMessage
                                              {
                                                  Command = aggregateRootEventStream,
                                                  SynchronizationKey = clientKey
                                              };
                            ErrorCodes returnCode = client.Process(message);
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                       aggregateRootEventStream.SourceId,
                                                                       returnCode == ErrorCodes.None
                                                                             ? EventState.Completed
                                                                             : EventState.Error));

                        }
                        invoker.Execute(new EndProcessComand(this.processGuid));
                    }
                );
        }

        public void Import()
        {
            ListOfAggregateRootsForImportMessage result = null;
            this.chanelFactoryWrapper.Execute<IGetAggragateRootList>(this.baseAdress,
                (client) =>
                {
                    result = client.Process();
                });
            if (result == null)
                throw new Exception("aggregate roots list is empty");
            List<AggregateRootEventStream> events=new List<AggregateRootEventStream>();
            this.chanelFactoryWrapper.Execute<IGetEventStream>(this.baseAdress, (client) =>
                                                                                    {
                                                                                        foreach (
                                                                                            Guid guid in result.Roots)
                                                                                        {
                                                                                            events.Add(
                                                                                                client.Process(guid).EventStream);

                                                                                        }
                                                                                    });
            this.eventStore.WriteEvents(events);
        }
    }
}
