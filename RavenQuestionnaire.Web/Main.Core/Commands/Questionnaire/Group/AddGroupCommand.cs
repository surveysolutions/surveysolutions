// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddGroupCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The add group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The add group command.
    /// </summary>
    [Obsolete]
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "AddGroup")]
    public class AddGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddGroupCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="parentGroupKey">
        /// The parent group key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public AddGroupCommand(
            Guid questionnaireId, Guid publicKey, string text, Guid? parentGroupKey, string conditionExpression, string description, Propagate propagateble)
        {
            this.QuestionnaireId = questionnaireId;
            this.PublicKey = publicKey;
            this.Text = text;
            this.ConditionExpression = conditionExpression;
            this.ParentGroupKey = parentGroupKey;
            this.Description = description;
            this.Propagateble = propagateble;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the parent group key.
        /// </summary>
        public Guid? ParentGroupKey { get; set; }

        /// <summary>
        /// Gets or sets the propagatable.
        /// </summary>
        public Propagate Propagateble { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        #endregion
    }
}