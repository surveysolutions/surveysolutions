// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupUpdated.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group updated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The group updated.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:GroupUpdated")]
    public class GroupUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the executor.
        /// </summary>
        public UserLight Executor { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the group text.
        /// </summary>
        public string GroupText { get; set; }

        /// <summary>
        /// Gets or sets the propagateble.
        /// </summary>
        public Propagate Propagateble { get; set; }

        // public List<Guid> Triggers { get; set; }
        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        #endregion
    }
}