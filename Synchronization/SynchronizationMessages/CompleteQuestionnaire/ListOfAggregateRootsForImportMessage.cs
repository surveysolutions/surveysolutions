namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System.Collections.Generic;
    using System.IO;

    using Main.Core.Documents;

    using Newtonsoft.Json;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The list of aggregate roots for import message.
    /// </summary>
    public class ListOfAggregateRootsForImportMessage : ICustomSerializable
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the roots.
        /// </summary>
        public IList<ProcessedEventChunk> Roots { get; set; }

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
            string rootsString = FormatHelper.ReadString(stream);
            this.Roots = JsonConvert.DeserializeObject<IList<ProcessedEventChunk>>(rootsString, settings);
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

            string rootsString = JsonConvert.SerializeObject(this.Roots, Formatting.None, settings);
            FormatHelper.WriteString(stream, rootsString);
        }

        #endregion
    }
}