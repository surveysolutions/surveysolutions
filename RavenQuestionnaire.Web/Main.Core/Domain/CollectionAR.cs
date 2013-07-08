namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Collection;

    using Ncqrs;
    using Ncqrs.Domain;

    /// <summary>
    /// The collection ar.
    /// </summary>
    public class CollectionAR : AggregateRootMappedByConvention
    {
        #region Fields

        /// <summary>
        /// The creation date.
        /// </summary>
        private DateTime creationDate;

        /// <summary>
        /// The items.
        /// </summary>
        private List<CollectionItem> items;

        /// <summary>
        /// The title.
        /// </summary>
        private string title;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionAR"/> class.
        /// </summary>
        public CollectionAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionAR"/> class.
        /// </summary>
        /// <param name="collectionId">
        /// The collection id.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        public CollectionAR(Guid collectionId, string text, List<CollectionItem> items)
            : base(collectionId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            this.ApplyEvent(
                new NewCollectionCreated
                    {
                       CollectionId = collectionId, Title = text, Items = items, CreationDate = clock.UtcNow() 
                    });
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
        protected void OnNewQuestionnaireCreated(NewCollectionCreated e)
        {
            this.title = e.Title;
            this.creationDate = e.CreationDate;
            this.items = e.Items;
        }

        #endregion
    }
}