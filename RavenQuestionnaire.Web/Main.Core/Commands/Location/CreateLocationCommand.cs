namespace Main.Core.Commands.Location
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The create location command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(LocationAR))]
    public class CreateLocationCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateLocationCommand"/> class.
        /// </summary>
        /// <param name="locationId">
        /// The location id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        public CreateLocationCommand(Guid locationId, string title)
        {
            this.LocationId = locationId;
            this.Text = title;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        public Guid LocationId { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        #endregion
    }
}