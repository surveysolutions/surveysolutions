// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddQuestionCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The add question command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire.Question
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The add question command.
    /// </summary>
    [Obsolete]
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "AddQuestion")]
    public class AddQuestionCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddQuestionCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="questionText">
        /// The question text.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="questionType">
        /// The question type.
        /// </param>
        /// <param name="questionScope">
        /// The question scope.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <param name="validationExpression">
        /// The validation expression.
        /// </param>
        /// <param name="validationMessage">
        /// The validation message.
        /// </param>
        /// <param name="instructions">
        /// The instructions.
        /// </param>
        /// <param name="featured">
        /// The featured.
        /// </param>
        /// <param name="mandatory">
        /// The mandatory.
        /// </param>
        /// <param name="answerOrder">
        /// The answer order.
        /// </param>
        /// <param name="answers">
        /// The answers.
        /// </param>
        /// <param name="maxValue">
        /// The max Value.
        /// </param>
        public AddQuestionCommand(Guid questionnaireId, Guid publicKey, string questionText, string stataExportCaption, QuestionType questionType, QuestionScope questionScope, Guid? groupPublicKey, string conditionExpression, string validationExpression, string validationMessage, string instructions, bool featured, bool mandatory, bool capital, Order answerOrder, Answer[] answers, int maxValue)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.QuestionScope = questionScope;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.Capital = capital;
            this.AnswerOrder = answerOrder;
            this.GroupPublicKey = groupPublicKey;
            this.Answers = answers;
            this.PublicKey = publicKey;
            this.MaxValue = maxValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddQuestionCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        ///   The questionnaire id.
        /// </param>
        /// <param name="publicKey">
        ///   The public key.
        /// </param>
        /// <param name="questionText">
        ///   The question text.
        /// </param>
        /// <param name="triggers">
        ///   The List of guids for autopropogated groups
        /// </param>
        /// <param name="maxValue">
        /// The question max value
        /// </param>
        /// <param name="stataExportCaption">
        ///   The stata export caption.
        /// </param>
        /// <param name="questionType">
        ///   The question type.
        /// </param>
        /// <param name="questionScope">
        ///   The question scope.
        /// </param>
        /// <param name="groupPublicKey">
        ///   The group public key.
        /// </param>
        /// <param name="conditionExpression">
        ///   The condition expression.
        /// </param>
        /// <param name="validationExpression">
        ///   The validation expression.
        /// </param>
        /// <param name="validationMessage">
        ///   The validation message.
        /// </param>
        /// <param name="instructions">
        ///   The instructions.
        /// </param>
        /// <param name="featured">
        ///   The featured.
        /// </param>
        /// <param name="mandatory">
        ///   The mandatory.
        /// </param>
        /// <param name="capital">
        /// The capital
        /// </param>
        /// <param name="answerOrder">
        ///   The answer order.
        /// </param>
        /// <param name="answers">
        ///   The answers.
        /// </param>
        public AddQuestionCommand(Guid questionnaireId, Guid publicKey, string questionText, List<Guid> triggers, int maxValue, string stataExportCaption, QuestionType questionType, QuestionScope questionScope, Guid? groupPublicKey, string conditionExpression, string validationExpression, string validationMessage, string instructions, bool featured, bool mandatory, bool capital, Order answerOrder, Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.Triggers = triggers;
            this.MaxValue = maxValue;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.QuestionScope = questionScope;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Mandatory = mandatory;
            this.Capital = capital;
            this.AnswerOrder = answerOrder;
            this.GroupPublicKey = groupPublicKey;
            this.Answers = answers;
            this.PublicKey = publicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets MaxValue.
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        public Answer[] Answers { get; set; }

        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid? GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        public bool Capital { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets QuestionScope.
        /// </summary>
        public QuestionScope QuestionScope { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

        ///// <summary>
        ///// Gets or sets the target group key.
        ///// </summary>
        //public Guid TargetGroupKey { get; set; }

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }

        #endregion

    }
}