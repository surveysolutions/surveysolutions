// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateGroupCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The update group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The update group command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateGroup")]
    public class UpdateGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateGroupCommand"/> class.
        /// </summary>
        /// <param name="groupText">
        /// The group text.
        /// </param>
        /// <param name="propagateble">
        /// The propagateble.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="executor">
        /// The executor.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        public UpdateGroupCommand(
            string groupText, 
            Propagate propagateble, 
            Guid questionnaireId, 
            Guid groupPublicKey, 
            UserLight executor, 
            string conditionExpression)
        {
            this.QuestionnaireId = questionnaireId;
            this.GroupText = groupText;
            this.Propagateble = propagateble;
            this.GroupPublicKey = groupPublicKey;
            this.ConditionExpression = conditionExpression;
            this.Executor = executor;
        }

        #endregion

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

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}