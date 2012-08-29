// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteGroupCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The delete group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The delete group command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteGroup")]
    public class DeleteGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteGroupCommand"/> class.
        /// </summary>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public DeleteGroupCommand(Guid groupPublicKey, Guid questionnaireId)
        {
            this.GroupPublicKey = groupPublicKey;
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}