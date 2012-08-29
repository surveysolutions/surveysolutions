// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveCollectionItemCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the RemoveCollectionItemCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Collection
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

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