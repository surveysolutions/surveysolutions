// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatableGroupAdded.cs" company="The World Bank">
//   2012
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
    /// The propagate group added.
    /// </summary>
    [Serializable]
    public class PropagatableGroupAdded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the completed questionnaire id.
        /// </summary>
        [Obsolete]
        public Guid CompletedQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question propagation key.
        /// </summary>
        public Guid? QuestionPropagationKey { get; set; }

        #endregion
    }
}