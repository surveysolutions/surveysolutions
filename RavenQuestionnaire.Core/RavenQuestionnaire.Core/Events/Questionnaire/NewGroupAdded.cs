// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewGroupAdded.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The new group added.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The new group added.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewGroupAdded")]
    public class NewGroupAdded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the group text.
        /// </summary>
        public string GroupText { get; set; }

        /// <summary>
        /// Gets or sets the parent group public key.
        /// </summary>
        public Guid? ParentGroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the paropagateble.
        /// </summary>
        public Propagate Paropagateble { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }

        #endregion
    }
}