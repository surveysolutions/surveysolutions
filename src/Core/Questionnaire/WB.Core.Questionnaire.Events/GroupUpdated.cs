// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupUpdated.cs" company="">
//   
// </copyright>
// <summary>
//   The group updated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The group updated.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:GroupUpdated")]
    public class GroupUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /*/// <summary>
        /// Gets or sets the executor.
        /// </summary>
        public UserLight Executor { get; set; }*/

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

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }
        #warning why do we use string here, not guid?

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}