// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveCollectionCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The remove collection command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Collection
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

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