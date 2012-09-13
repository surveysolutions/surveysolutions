// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatableGroupAdded.cs" company="">
//   
// </copyright>
// <summary>
//   The propagatable group added.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The propagatable group added.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:PropagatableGroupAdded")]
    public class PropagatableGroupAdded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}