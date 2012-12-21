namespace Core.Supervisor.Views.Timeline
{
    using System;

    using Main.Core.View.Question;

    /// <summary>
    /// Timeline Question
    /// </summary>
    public class TimelineQuestion
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineQuestion"/> class.
        /// </summary>
        /// <param name="questionView">
        /// The question view.
        /// </param>
        public TimelineQuestion(CompleteQuestionView questionView)
        {
            this.QuestionView = questionView;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        public DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets the approximate time.
        /// </summary>
        public TimeSpan? ApproximateTime { get; set; }

        /// <summary>
        /// Gets or sets QuestionView.
        /// </summary>
        public CompleteQuestionView QuestionView { get; set; }

        #endregion
    }
}