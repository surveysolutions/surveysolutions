// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatableGroupDeleted.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagatable group deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The propagatable group deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:PropagatableGroupDeleted")]
    public class PropagatableGroupDeleted
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