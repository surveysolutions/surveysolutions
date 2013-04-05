// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoveQuestionnaireItemCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The move questionnaire item command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The move questionnaire item command.
    /// </summary>
    [Obsolete]
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "MoveQuestionnaireItem")]
    public class MoveQuestionnaireItemCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveQuestionnaireItemCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="afterItemKey">
        /// The after item key.
        /// </param>
        public MoveQuestionnaireItemCommand(Guid questionnaireId, Guid publicKey, Guid? groupKey, Guid? afterItemKey)
        {
            this.QuestionnaireId = questionnaireId;
            this.PublicKey = publicKey;
            this.AfterItemKey = afterItemKey;
            this.GroupKey = groupKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the after item key.
        /// </summary>
        public Guid? AfterItemKey { get; set; }

        /// <summary>
        /// Gets or sets the group key.
        /// </summary>
        public Guid? GroupKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}