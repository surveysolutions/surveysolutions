// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewQuestionAdded.cs" company="">
//   
// </copyright>
// <summary>
//   The new question added.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new question added.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewQuestionAdded")]
    public class NewQuestionAdded
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        public IEnumerable<IAnswer> Answers { get; set; }

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
        /// Gets or sets the stata export caption.
        /// </summary>
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        [Obsolete]
        public Guid TargetGroupKey { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        /// <summary>
        /// Gets or sets MaxValue.
        /// </summary>
        public int MaxValue { get; set; }

        #endregion
    }
}