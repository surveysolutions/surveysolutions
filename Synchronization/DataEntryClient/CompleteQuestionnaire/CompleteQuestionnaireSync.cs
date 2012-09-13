// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireSync.cs" company="">
//   
// </copyright>
// <summary>
//   The complete questionnaire sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.WcfInfrastructure;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using SynchronizationMessages.CompleteQuestionnaire;
    using SynchronizationMessages.Handshake;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public class CompleteQuestionnaireSync : ICompleteQuestionnaireSync
    {
        #region Fields

        /// <summary>
        /// The base adress.
        /// </summary>
        private readonly string baseAdress;

        /// <summary>
        /// The chanel factory wrapper.
        /// </summary>
        private readonly IChanelFactoryWrapper chanelFactoryWrapper;

        // private IClientSettingsProvider clientSettingsProvider;
        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventSync eventStore;

        /// <summary>
        /// The invoker.
        /// </summary>
        private readonly ICommandService invoker;

        /// <summary>
        /// The process guid.
        /// </summary>
        private readonly Guid processGuid;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireSync"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="processGuid">
        /// The process guid.
        /// </param>
        /// <param name="baseAdress">
        /// The base adress.
        /// </param>
        public CompleteQuestionnaireSync(IKernel kernel, Guid processGuid, string baseAdress)
        {
            this.chanelFactoryWrapper = kernel.Get<IChanelFactoryWrapper>();

            // this.clientSettingsProvider = kernel.Get<IClientSettingsProvider>();
            this.eventStore = kernel.Get<IEventSync>();
            this.invoker = NcqrsEnvironment.Get<ICommandService>();
            this.processGuid = processGuid;
            this.baseAdress = baseAdress;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        public void Export(Guid syncKey)
        {
            try
            {
                // TODO: uncomment that string if we'll be synchronizing delta instead of everything   Guid? lastSyncEventGuid = GetLastSyncEventGuid(syncKey);
                this.UploadEvents(syncKey, null);
            }
            catch (Exception)
            {
                this.invoker.Execute(new EndProcessComand(this.processGuid, EventState.Error));
            }
        }

        /// <summary>
        /// The get last sync event guid.
        /// </summary>
        /// <param name="clientKey">
        /// The client key.
        /// </param>
        /// <returns>
        /// The System.Nullable`1[T -&gt; System.Guid].
        /// </returns>
        public Guid? GetLastSyncEventGuid(Guid clientKey)
        {
            Guid? result = null;
            this.chanelFactoryWrapper.Execute<IGetLastSyncEvent>(
                this.baseAdress, (client) => { result = client.Process(clientKey); });
            return result;
        }

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void Import(Guid syncKey)
        {
            try
            {
                ListOfAggregateRootsForImportMessage result = null;
                this.chanelFactoryWrapper.Execute<IGetAggragateRootList>(
                    this.baseAdress, (client) => { result = client.Process(); });
                if (result == null)
                {
                    throw new Exception("aggregate roots list is empty");
                }

                this.invoker.Execute(new PushEventsCommand(this.processGuid, result.Roots));
                var events = new List<AggregateRootEvent>();
                this.chanelFactoryWrapper.Execute<IGetEventStream>(
                    this.baseAdress, 
                    (client) =>
                        {
                            foreach (ProcessedEventChunk root in result.Roots)
                            {
                                try
                                {
                                    if (root.EventKeys.Count == 0)
                                    {
                                        continue;
                                    }

                                    AggregateRootEvent[] stream =
                                        client.Process(root.EventKeys.First(), root.EventKeys.Count).EventStream;
                                    events.AddRange(stream);
                                    this.invoker.Execute(
                                        new ChangeEventStatusCommand(
                                            this.processGuid, root.EventChunckPublicKey, EventState.Completed));
                                }
                                catch (Exception)
                                {
                                    events = null;
                                    this.invoker.Execute(
                                        new ChangeEventStatusCommand(
                                            this.processGuid, root.EventChunckPublicKey, EventState.Error));
                                }
                            }
                        });
                if (events == null)
                {
                    return;
                }

                this.eventStore.WriteEvents(events);
                this.invoker.Execute(new EndProcessComand(this.processGuid, EventState.Completed));
            }
            catch (Exception)
            {
                this.invoker.Execute(new EndProcessComand(this.processGuid, EventState.Error));
            }
        }

        /// <summary>
        /// The upload events.
        /// </summary>
        /// <param name="clientKey">
        /// The client key.
        /// </param>
        /// <param name="lastSyncEvent">
        /// The last sync event.
        /// </param>
        public void UploadEvents(Guid clientKey, Guid? lastSyncEvent)
        {
            this.chanelFactoryWrapper.Execute<IEventPipe>(
                this.baseAdress, 
                (client) =>
                    {
                        var events = this.eventStore.ReadEventsByChunks().ToList();
                        var command = new PushEventsCommand(this.processGuid, events);
                        this.invoker.Execute(command);
                        for (int i = 0; i < events.Count; i++)
                        {
                            this.invoker.Execute(
                                new ChangeEventStatusCommand(
                                    this.processGuid, 
                                    command.EventChuncks[i].EventChunckPublicKey, 
                                    EventState.InProgress));
                            var message = new EventSyncMessage
                                {
                                   Command = events[i].ToArray(), SynchronizationKey = clientKey 
                                };
                            ErrorCodes returnCode = client.Process(message);
                            this.invoker.Execute(
                                new ChangeEventStatusCommand(
                                    this.processGuid, 
                                    command.EventChuncks[i].EventChunckPublicKey, 
                                    returnCode == ErrorCodes.None ? EventState.Completed : EventState.Error));
                        }

                        this.invoker.Execute(new EndProcessComand(this.processGuid, EventState.Completed));
                    });
        }

        #endregion
    }
}