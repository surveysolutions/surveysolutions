namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System;
    using System.IO;

    using Main.Core.Events;

    using Newtonsoft.Json;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The event sync message.
    /// </summary>
    public class EventSyncMessage : ICustomSerializable
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public AggregateRootEvent[] Command { get; set; }

        /// <summary>
        /// Gets or sets the synchronization key.
        /// </summary>
        public Guid SynchronizationKey { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The initialize from.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void InitializeFrom(Stream stream)
        {
            this.SynchronizationKey = FormatHelper.ReadGuid(stream);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            string commandString = FormatHelper.ReadString(stream);
            var command = JsonConvert.DeserializeObject<AggregateRootEvent[]>(commandString, settings);

            this.Command = command;
        }

        /// <summary>
        /// The write to.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void WriteTo(Stream stream)
        {
            FormatHelper.WriteGuid(stream, this.SynchronizationKey);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            string command = JsonConvert.SerializeObject(this.Command, Formatting.None, settings);
            FormatHelper.WriteString(stream, command);
        }

        #endregion
    }
}