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
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Synchronization;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Handshake;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync : ICompleteQuestionnaireSync
    {
        private IChanelFactoryWrapper chanelFactoryWrapper;
      //  private IClientSettingsProvider clientSettingsProvider;
        private IEventSync eventStore;
        private Guid processGuid;
        private string baseAdress;
        private ICommandService invoker;
        public CompleteQuestionnaireSync(IKernel kernel, Guid processGuid, string baseAdress)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();
       //     this.clientSettingsProvider = kernel.Get<IClientSettingsProvider>();
            this.eventStore = kernel.Get<IEventSync>();
            this.invoker = NcqrsEnvironment.Get<ICommandService>();
            this.processGuid = processGuid;
            this.baseAdress = baseAdress;
        }

        public void Export(Guid syncKey)
        {
            try
            {
             // TODO: uncomment that string if we'll be synchronizing delta instead of everything   Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);

                UploadEvents(syncKey, null);
            }
            catch (Exception)
            {

                invoker.Execute(new EndProcessComand(this.processGuid, EventState.Error));
            }
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
                        var events = this.eventStore.ReadEvents().ToList();
                        var command = new PushEventsCommand(this.processGuid, events);
                        invoker.Execute(command);
                        foreach (var chunk in command.AggregateRoots)
                        {
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                         chunk.EventChunckPublicKey,
                                                                         EventState.InProgress));
                            var message = new EventSyncMessage
                                              {
                                                  Command = events.SkipWhile(e=>e.EventIdentifier!= chunk.EventKeys.First()).Take(chunk.EventKeys.Count).ToArray(),
                                                  SynchronizationKey = clientKey
                                              };
                            ErrorCodes returnCode = client.Process(message);
                            invoker.Execute(new ChangeEventStatusCommand(this.processGuid,
                                                                       chunk.EventChunckPublicKey,
                                                                       returnCode == ErrorCodes.None
                                                                             ? EventState.Completed
                                                                             : EventState.Error));

                        }
                        invoker.Execute(new EndProcessComand(this.processGuid, EventState.Completed));
                    }
                );
        }

        public void Import(Guid syncKey)
        {
            try
            {
                ListOfAggregateRootsForImportMessage result = null;
                this.chanelFactoryWrapper.Execute<IGetAggragateRootList>(this.baseAdress,
                                                                         (client) =>
                                                                             {
                                                                                 result = client.Process();
                                                                             });
                if (result == null)
                    throw new Exception("aggregate roots list is empty");
                invoker.Execute(new PushEventsCommand(this.processGuid,result.Roots));
                List<AggregateRootEvent> events = new List<AggregateRootEvent>();
                this.chanelFactoryWrapper.Execute<IGetEventStream>(this.baseAdress, (client) =>
                                                                                        {
                                                                                            foreach (
                                                                                                var  root in
                                                                                                    result.Roots)
                                                                                            {
                                                                                                try
                                                                                                {

                                                                                                if(root.EventKeys.Count==0)
                                                                                                    continue;
                                                                                                var stream = client.Process(root.EventKeys.First(), root.EventKeys.Count).EventStream;
                                                                                                    events.AddRange(stream);
                                                                                                invoker.Execute(
                                                                                                    new ChangeEventStatusCommand
                                                                                                        (this.processGuid, root.EventChunckPublicKey, EventState.Completed));
                                                                                                }
                                                                                                catch (Exception)
                                                                                                {

                                                                                                    invoker.Execute(
                                                                                                    new ChangeEventStatusCommand
                                                                                                        (this.processGuid, root.EventChunckPublicKey, EventState.Error));
                                                                                                }
                                                                                            }
                                                                                        });
                this.eventStore.WriteEvents(events);
                invoker.Execute(new EndProcessComand(this.processGuid, EventState.Completed));
            }
            catch (Exception)
            {

                invoker.Execute(new EndProcessComand(this.processGuid, EventState.Error));
            }
        }
    }
}
