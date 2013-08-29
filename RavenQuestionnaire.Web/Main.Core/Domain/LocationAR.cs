namespace Main.Core.Domain
{
    using System;

    using Main.Core.Events.Location;

    using Ncqrs;
    using Ncqrs.Domain;

    /// <summary>
    /// The location AR.
    /// </summary>
    public class LocationAR : AggregateRootMappedByConvention
    {
        #region Fields

        /// <summary>
        /// The creation date.
        /// </summary>
        private DateTime creationDate;

        /// <summary>
        /// The title.
        /// </summary>
        private string title;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationAR"/> class.
        /// </summary>
        public LocationAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationAR"/> class.
        /// </summary>
        /// <param name="locationId">
        /// The location id.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public LocationAR(Guid locationId, string text)
            : base(locationId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            this.ApplyEvent(
                new NewLocationCreated { LocationId = locationId, Title = text, CreationDate = clock.UtcNow() });
        }

        #endregion

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        #region Methods

        /// <summary>
        /// The on new questionnaire created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionnaireCreated(NewLocationCreated e)
        {
            this.title = e.Title;
            this.creationDate = e.CreationDate;
        }

        #endregion
    }
}