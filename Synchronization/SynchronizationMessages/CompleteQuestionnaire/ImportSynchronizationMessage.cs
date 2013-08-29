namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System.IO;

    using Main.Core.Events;

    using Newtonsoft.Json;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The import synchronization message.
    /// </summary>
    public class ImportSynchronizationMessage : ICustomSerializable
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the event stream.
        /// </summary>
        public AggregateRootEvent[] EventStream { get; set; }

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
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            
            string commandString = FormatHelper.ReadString(stream);
            var command = JsonConvert.DeserializeObject<AggregateRootEvent[]>(commandString, settings);

            this.EventStream = command;
        }

        /// <summary>
        /// The write to.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public void WriteTo(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            string command = JsonConvert.SerializeObject(this.EventStream, Formatting.None, settings);
            FormatHelper.WriteString(stream, command);
        }

        #endregion
    }
}