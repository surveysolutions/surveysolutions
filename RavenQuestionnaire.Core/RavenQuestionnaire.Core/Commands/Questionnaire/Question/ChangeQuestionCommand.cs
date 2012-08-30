// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeQuestionCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The change question command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// The change question command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "ChangeQuestion")]
    public class ChangeQuestionCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeQuestionCommand"/> class.
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
        /// <param name="capital">
        /// The capital
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
        public ChangeQuestionCommand(
            Guid questionnaireId, 
            Guid publicKey, 
            string questionText, 
            string stataExportCaption, 
            QuestionType questionType, 
            string conditionExpression, 
            string validationExpression, 
            string validationMessage, 
            string instructions, 
            bool featured,
            bool capital, 
            bool mandatory, 
            Order answerOrder, 
            Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Capital = capital;
            this.Mandatory = mandatory;
            this.AnswerOrder = answerOrder;
            this.PublicKey = publicKey;
            this.Answers = answers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeQuestionCommand"/> class.
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
        /// <param name="targetGroupKey">
        /// The target group key.
        /// </param>
        /// <param name="stataExportCaption">
        /// The stata export caption.
        /// </param>
        /// <param name="questionType">
        /// The question type.
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
        /// <param name="capital">
        /// The capital
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
        public ChangeQuestionCommand(
            Guid questionnaireId, 
            Guid publicKey, 
            string questionText, 
            Guid targetGroupKey, 
            string stataExportCaption, 
            QuestionType questionType, 
            string conditionExpression, 
            string validationExpression, 
            string validationMessage, 
            string instructions, 
            bool featured, 
            bool capital,
            bool mandatory, 
            Order answerOrder, 
            Answer[] answers)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionText = questionText;
            this.TargetGroupKey = targetGroupKey;
            this.StataExportCaption = stataExportCaption;
            this.QuestionType = questionType;
            this.ConditionExpression = conditionExpression;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Featured = featured;
            this.Capital = capital;
            this.Mandatory = mandatory;
            this.AnswerOrder = answerOrder;
            this.PublicKey = publicKey;
            this.Answers = answers;
        }

        #endregion

        #region Public Properties

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
        /// Gets or sets a value indicating whether capital.
        /// </summary>
        public bool Capital { get; set; }


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
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        public Guid TargetGroupKey { get; set; }

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