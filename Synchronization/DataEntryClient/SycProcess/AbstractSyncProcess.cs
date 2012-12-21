﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.ServiceModel;

    using DataEntryClient.SycProcess.Interfaces;
    using DataEntryClient.SycProcessRepository;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using NLog;

    using SynchronizationMessages.CompleteQuestionnaire;

    using LogManager = NLog.LogManager;

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
        protected readonly IEventStreamReader EventStore;

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
            this.EventStore = kernel.Get<IEventStreamReader>();
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

                var syncProcess = this.SyncProcessRepository.GetProcess(this.ProcessGuid);

                syncProcess.Merge(events);

                var statistics = syncProcess.CalculateStatistics();

                this.Invoker.Execute(new PushStatisticsCommand(this.ProcessGuid, statistics));

                syncProcess.Commit();

                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));
            }
            catch (Exception ex)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Fatal("Import error: " + ex.Message, ex);
                this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Error, ex.Message));
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
                Logger logger = LogManager.GetCurrentClassLogger();
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
        protected ErrorCodes ProcessEvents(IEventPipe client)
        {
            ErrorCodes returnCode = ErrorCodes.None;
            var events = this.EventStore.ReadEventsByChunks().ToList();
            var command = new PushEventsCommand(this.ProcessGuid, events);
            this.Invoker.Execute(command);
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