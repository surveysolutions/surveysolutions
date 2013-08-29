namespace Main.Core.Commands.Collection
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The remove collection item command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CollectionAR), "RemoveCollectionItem")]
    public class RemoveCollectionItemCommand : CommandBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the collection id.
        /// </summary>
        public Guid CollectionId { get; set; }

        #endregion
    }
}