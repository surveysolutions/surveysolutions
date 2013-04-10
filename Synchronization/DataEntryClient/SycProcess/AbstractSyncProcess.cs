// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractSyncProcess.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The abstract sync process.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DataEntryClient.SycProcess.Interfaces;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;
    using Main.Synchronization.SycProcessRepository;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NLog;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// The complete questionnaire sync.
    /// </summary>
    public abstract class AbstractSyncProcess : ISyncProcess
    {
        #region Constants and Fields

        /// <summary>
        /// The process guid.
        /// </summary>
        protected readonly Guid ProcessGuid;

        /// <summary>
        /// The parent process guid.
        /// </summary>
        public readonly Guid? ParentProcessGuid;

        /// <summary>
        /// The invoker.
        /// </summary>
        protected readonly ICommandService Invoker;

        /// <summary>
        /// The event store.
        /// </summary>
        protected readonly IEventStreamReader EventStoreReader;

        /// <summary>
        /// sync process repository
        /// </summary>
        protected readonly ISyncProcessRepository SyncProcessRepository;

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
        /// <param name="parentSyncProcess">
        /// The parent Sync Process.
        /// </param>
        protected AbstractSyncProcess(IKernel kernel, Guid syncProcess, Guid? parentSyncProcess = null)
        {
            this.EventStoreReader = kernel.Get<IEventStreamReader>();
            this.Invoker = NcqrsEnvironment.Get<ICommandService>();
            this.ProcessGuid = syncProcess;
            this.ParentProcessGuid = parentSyncProcess;
            this.SyncProcessRepository = kernel.Get<ISyncProcessRepository>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The import.
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <exception cref="Exception">
        /// Some exception
        /// </exception>
        /// <returns>
        /// The ErrorCodes.
        /// </returns>
        public ErrorCodes Import(string syncProcessDescription)
        {
            this.Invoker.Execute(new CreateNewSynchronizationProcessCommand(this.ProcessGuid, this.ParentProcessGuid, SynchronizationType.Pull, syncProcessDescription));
            try
            {
                var events = this.GetEventStream();
                if (events == null)
                {
                    this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error, "Fail. No events"));
                    return ErrorCodes.Fail;
                }

                var syncProcess = this.SyncProcessRepository.GetProcessor(this.ProcessGuid);

                syncProcess.Merge(events);

                syncProcess.PostProcess();
               

                syncProcess.Commit();

                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));
            }
            catch (Exception ex)
            {
                Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal("Import error: " + ex.Message, ex);
                this.Invoker.Execute(
                    new EndProcessComand(
                        this.ProcessGuid,
                        EventState.Error,
                        (ex.InnerException != null) ? ex.Message + "Inner: " + ex.InnerException.Message : ex.Message));
                
                return ErrorCodes.Fail;
            }

            return ErrorCodes.None;
        }

        /// <summary>
        /// The export.
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <returns>
        /// The export
        /// </returns>
        public ErrorCodes Export(string syncProcessDescription)
        {
            this.Invoker.Execute(new CreateNewSynchronizationProcessCommand(this.ProcessGuid, this.ParentProcessGuid, SynchronizationType.Push, syncProcessDescription));
            try
            {
                this.ExportEvents();
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));
            }
            catch (Exception ex)
            {
                Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal("Import error", ex);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error, ex.Message));
                return ErrorCodes.Fail;
            }

            return ErrorCodes.None;
        }

        /// <summary>
        /// Export events method
        /// </summary>
        protected abstract void ExportEvents();

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
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The process events.
        /// </returns>
        protected virtual ErrorCodes ProcessEvents(IEventPipe client)
        {
            ErrorCodes returnCode = ErrorCodes.None;

            List<IEnumerable<AggregateRootEvent>> events = this.EventStoreReader.ReadEventsByChunks().ToList();

            this.Invoker.Execute(new PushEventsCommand(this.ProcessGuid, events));
            foreach (IEnumerable<AggregateRootEvent> t in events)
            {
                var message = new EventSyncMessage { Command = t.ToArray(), SynchronizationKey = this.ProcessGuid };
                returnCode = client.Process(message);
            }

            return returnCode;
        }

        #endregion
    }
}