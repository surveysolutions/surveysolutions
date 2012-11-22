// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractSyncProcess.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The abstract sync process.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Core.Supervisor.Synchronization;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.Events.User;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Restoring.EventStapshoot;

    using Ninject;

    using NLog;

    using SynchronizationMessages.CompleteQuestionnaire;

    using LogManager = NLog.LogManager;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public abstract class AbstractSyncProcess : ICompleteQuestionnaireSync
    {
        #region Constants and Fields

        /// <summary>
        /// The process guid.
        /// </summary>
        protected readonly Guid ProcessGuid;

        /// <summary>
        /// The invoker.
        /// </summary>
        protected readonly ICommandService Invoker;

        /// <summary>
        /// The event store.
        /// </summary>
        protected readonly IEventSync EventStore;

        /// <summary>
        /// The user store
        /// </summary>
        protected readonly IUserEventSync UserStore;


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSyncProcess"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcess">
        /// Sync Process Guid
        /// </param>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        protected AbstractSyncProcess(IKernel kernel, Guid syncProcess)
        {
            this.EventStore = kernel.Get<IEventSync>();
            this.Invoker = NcqrsEnvironment.Get<ICommandService>();
            this.ProcessGuid = syncProcess;
            this.UserStore = kernel.Get<IUserEventSync>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        public void Import(Guid syncKey)
        {
            this.Invoker.Execute(new CreateNewSynchronizationProcessCommand(this.ProcessGuid, SynchronizationType.Pull));
            try
            {
                //syncKey = Guid.Parse("15a5f1ac-4d9d-4531-9b23-ff29ab8a1686");
                var events = this.EventStore.ReadEvents(syncKey);
                if (events == null)
                {
                    return;
                }
                this.EventStore.WriteEvents(events);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed));
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal("Import error: " + ex.Message, ex);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error));
            }
        }

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        public void Export(Guid syncKey)
        {
            this.Invoker.Execute(new CreateNewSynchronizationProcessCommand(this.ProcessGuid, SynchronizationType.Push));
            try
            {
                this.ExportEvents(syncKey);
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal("Import error", ex);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error));
            }
        }

        /// <summary>
        /// Export events method
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        protected abstract void ExportEvents(Guid syncKey);

        /// <summary>
        /// Gets list of event to syncronize
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        protected abstract IEnumerable<AggregateRootEvent> GetEventStream();

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        protected void ProcessEvents(Guid syncKey, IEventPipe client)
        {
            var events = this.EventStore.ReadEventsByChunks().ToList();
            var command = new PushEventsCommand(this.ProcessGuid, events);
            this.Invoker.Execute(command);
            for (int i = 0; i < events.Count; i++)
            {
                this.Invoker.Execute(new ChangeEventStatusCommand(this.ProcessGuid, command.EventChuncks[i].EventChunckPublicKey, EventState.InProgress));
                var message = new EventSyncMessage { Command = events[i].ToArray(), SynchronizationKey = syncKey };
                ErrorCodes returnCode = client.Process(message);
                this.Invoker.Execute(new ChangeEventStatusCommand(this.ProcessGuid, command.EventChuncks[i].EventChunckPublicKey, returnCode == ErrorCodes.None ? EventState.Completed : EventState.Error));
            }

            this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed));
        }

        #endregion
    }
}