// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetAnswerCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the SetAnswerCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    using System;
    using System.Collections.Generic;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.Question;

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
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        public SetAnswerCommand(Guid completeQuestionnaireId, CompleteQuestionView question, Guid? propogationPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.PropogationPublicKey = propogationPublicKey;
            this.QuestionPublickey = question.PublicKey;

            if (question.QuestionType == QuestionType.DropDownList || question.QuestionType == QuestionType.SingleOption
                || question.QuestionType == QuestionType.YesNo || question.QuestionType == QuestionType.MultyOption)
            {
                if (question.Answers != null && question.Answers.Length > 0)
                {
                    var answers = new List<Guid>();
                    for (int i = 0; i < question.Answers.Length; i++)
                    {
                        if (question.Answers[i].Selected)
                        {
                            answers.Add(question.Answers[i].PublicKey);
                        }
                    }

                    this.CompleteAnswers = answers;
                }
            }
            else
            {
                this.CompleteAnswerValue = question.Answers[0].AnswerValue;
            }
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

        #endregion
    }
}