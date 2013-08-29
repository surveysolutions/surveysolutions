namespace Core.Supervisor.Views.Survey
{
    using System;

    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The question statistic view.
    /// </summary>
    public class QuestionStatisticView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionStatisticView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        public QuestionStatisticView(ICompleteQuestion doc, Guid gropPublicKey)
        {
            this.PublicKey = doc.PublicKey;
            this.QuestionText = doc.QuestionText;

            this.GroupPublicKey = gropPublicKey;
            this.GroupPropagationPublicKey = doc.PropagationPublicKey;
            this.ErrorMessage = doc.ValidationMessage;
            if (doc.PropagationPublicKey.HasValue)
            {
                this.IsQuestionFromPropGroup = true;
            }

            if (doc.IsAnswered())
            {
                this.AnswerDate = doc.AnswerDate;
                this.AnswerValue = this.AnswerText = doc.GetAnswerString();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        public DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the answer value.
        /// </summary>
        public string AnswerValue { get; set; }

        /// <summary>
        /// Gets or sets the approximate time.
        /// </summary>
        public TimeSpan? ApproximateTime { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the group propagation public key.
        /// </summary>
        public Guid? GroupPropagationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is question from prop group.
        /// </summary>
        public bool IsQuestionFromPropGroup { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        #endregion
    }
}