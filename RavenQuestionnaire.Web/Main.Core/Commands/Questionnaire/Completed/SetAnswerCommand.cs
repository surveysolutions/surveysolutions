namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Domain;

    using Ncqrs;
    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The set answer command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetAnswer")]
    public class SetAnswerCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetAnswerCommand"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="questionPublicKey">
        /// The question Public Key.
        /// </param>
        /// <param name="answersList">
        /// The answers List.
        /// </param>
        /// <param name="answerValue">
        /// The answer Value.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        public SetAnswerCommand(
            Guid completeQuestionnaireId,
            Guid questionPublicKey,
            List<Guid> answersList,
            string answerValue,
            Guid? propogationPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.PropogationPublicKey = propogationPublicKey;
            this.QuestionPublickey = questionPublicKey;
            this.CompleteAnswers = answersList;
            this.CompleteAnswerValue = answerValue;
            var clock = NcqrsEnvironment.Get<IClock>();
            this.AnswerDate = clock.UtcNow();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the complete answer value.
        /// </summary>
        public string CompleteAnswerValue { get; private set; }

        /// <summary>
        /// Gets the complete answers.
        /// </summary>
        public List<Guid> CompleteAnswers { get; private set; }

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        public Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question publickey.
        /// </summary>
        public Guid QuestionPublickey { get; set; }

        /// <summary>
        /// Gets or sets AnswerDate.
        /// </summary>
        public DateTime AnswerDate { get; set; }

        #endregion
    }
}