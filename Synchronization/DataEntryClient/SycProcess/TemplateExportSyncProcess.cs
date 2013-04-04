// -----------------------------------------------------------------------
// <copyright file="TemplateExportSyncProcess.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.SycProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Documents;
    using Main.Core.Events;

    using Ncqrs.Restoring.EventStapshoot;

    using Ninject;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class TemplateExportSyncProcess : UsbSyncProcess, ITemplateExportSyncProcess
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
        public TemplateExportSyncProcess(IKernel kernel, Guid syncProcess)
            : base(kernel, syncProcess)
        {
        }

        /// <summary>
        /// Gets or sets TemplateGuid.
        /// </summary>
        public Guid? TemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets ClientGuid.
        /// </summary>
        public Guid? ClientGuid { get; set; }

        [Obsolete("Export(string) is deprecated, please use Export(string, Guid?, Guid?) instead.", true)]
        public new byte[] Export(string syncProcessDescription)
        {
            return new byte[] { };
        }

        /// <summary>
        /// The export
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="templateGuid">
        /// The template guid.
        /// </param>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <returns>
        /// Zip file as byte array
        /// </returns>
        public byte[] Export(string syncProcessDescription, Guid? templateGuid, Guid? clientGuid)
        {
            this.TemplateGuid = templateGuid;
            this.ClientGuid = clientGuid;
            return base.Export(syncProcessDescription);
        }

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        protected override ErrorCodes ProcessEvents(IEventPipe client)
        {
            ErrorCodes returnCode = ErrorCodes.Fail;
            var archive = new List<AggregateRootEvent>();
            var events = this.EventStoreReader.ReadEvents().ToList();
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
                returnCode = client.Process(message);
                this.Invoker.Execute(new ChangeEventStatusCommand(this.ProcessGuid, command.EventChuncks[i].EventChunckPublicKey, returnCode == ErrorCodes.None ? EventState.Completed : EventState.Error));
            }

            return returnCode;
        }

    }
}
