// -----------------------------------------------------------------------
// <copyright file="TemplateExportSyncProcess.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs.Restoring.EventStapshoot;

    using Ninject;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class TemplateExportSyncProcess : UsbSyncProcess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExportSyncProcess"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcess">
        /// The sync process.
        /// </param>
        /// <param name="templateGuid">
        /// The template Guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client Guid.
        /// </param>
        public TemplateExportSyncProcess(IKernel kernel, Guid syncProcess, Guid? templateGuid, Guid? clientGuid)
            : base(kernel, syncProcess)
        {
            this.TemplateGuid = templateGuid;
            this.ClientGuid = clientGuid;
        }

        /// <summary>
        /// Gets or sets TemplateGuid.
        /// </summary>
        public Guid? TemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets ClientGuid.
        /// </summary>
        public Guid? ClientGuid { get; set; }

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        protected new void ProcessEvents(IEventPipe client)
        {
            var archive = new List<AggregateRootEvent>();
            var events = this.EventStore.ReadEvents().ToList();
            if (this.TemplateGuid != null)
                archive.Add(
                    events.Where(
                        t =>
                            {
                        var payload = (t.Payload as SnapshootLoaded).Template.Payload as QuestionnaireDocument;
                        return payload != null && payload.PublicKey == this.TemplateGuid;
                    }).FirstOrDefault());
            else
                archive.AddRange(
                    events.Where(ar => ar.Payload is SnapshootLoaded).Where(
                        aggregateRootEvent =>
                        ((SnapshootLoaded)(aggregateRootEvent.Payload)).Template.Payload is QuestionnaireDocument));

            var eventsList = new List<IEnumerable<AggregateRootEvent>> { archive };
            var command = new PushEventsCommand(this.ProcessGuid, eventsList);
            this.Invoker.Execute(command);
            for (int i = 0; i < eventsList.Count; i++)
            {
                this.Invoker.Execute(new ChangeEventStatusCommand(this.ProcessGuid, command.EventChuncks[i].EventChunckPublicKey, EventState.InProgress));
                var message = new EventSyncMessage { Command = eventsList[i].ToArray(), SynchronizationKey = this.ProcessGuid };
                ErrorCodes returnCode = client.Process(message);
                this.Invoker.Execute(new ChangeEventStatusCommand(this.ProcessGuid, command.EventChuncks[i].EventChunckPublicKey, returnCode == ErrorCodes.None ? EventState.Completed : EventState.Error));
            }

            this.Invoker.Execute(new EndProcessComand(this.ProcessGuid, EventState.Completed, "Ok"));
        }

    }
}
