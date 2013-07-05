namespace Main.Core.Commands.Collection
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The remove collection command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CollectionAR), "RemoveCollection")]
    public class RemoveCollectionCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        [AggregateRootId]
        public Guid CollectionId { get; set; }

        #endregion
    }
}