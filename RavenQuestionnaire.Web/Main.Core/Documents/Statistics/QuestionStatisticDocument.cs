namespace Main.Core.Documents.Statistics
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The question statistic document.
    /// </summary>
    public class QuestionStatisticDocument
    {
        /*private ICompleteQuestion completeQuestion;*/
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionStatisticDocument"/> class.
        /// </summary>
        public QuestionStatisticDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionStatisticDocument"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="gropPublicKey">
        /// The grop public key.
        /// </param>
        /// <param name="gropPropagationPublicKey">
        /// The grop propagation public key.
        /// </param>
        /// <param name="screenPublicKey">
        /// The screen public key.
        /// </param>
        public QuestionStatisticDocument(
            ICompleteQuestion question, Guid gropPublicKey, Guid? gropPropagationPublicKey, Guid screenPublicKey)
        {
            this.PublicKey = question.PublicKey;
            this.GroupPublicKey = gropPublicKey;
            this.GroupPropagationPublicKey = gropPropagationPublicKey;
            this.ScreenPublicKey = screenPublicKey;
            this.QuestionText = question.QuestionText;
            this.QuestionType = question.QuestionType;
            this.QuestionScope = question.QuestionScope;

            if (question.IsAnswered())
            {
                // AnswerValue = answer.AnswerText?? answer.AnswerValue;
                this.AnswerDate = question.AnswerDate;
                this.AnswerText = question.GetAnswerString();
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
        /// Gets or sets the approximate time.
        /// </summary>
        public TimeSpan? ApproximateTime { get; set; }

        /// <summary>
        /// Gets or sets the group propagation public key.
        /// </summary>
        public Guid? GroupPropagationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

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
        /// Gets or sets the screen public key.
        /// </summary>
        public Guid ScreenPublicKey { get; set; }

        #endregion

        // public object AnswerValue { get; set; }

        
    }
}