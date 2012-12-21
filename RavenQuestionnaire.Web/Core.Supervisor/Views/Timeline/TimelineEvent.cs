namespace Core.Supervisor.Views.Timeline
{
    using System;

    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Question;

    /// <summary>
    /// Timeline Event
    /// </summary>
    public class TimelineEvent
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineEvent"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="questionView">
        /// The question view.
        /// </param>
        public TimelineEvent(ICompleteQuestion question, CompleteQuestionView questionView)
        {
            this.StartDate = question.AnswerDate.Value;
            this.EndDate = question.AnswerDate.Value;
            this.Question = new TimelineQuestion(questionView) { AnswerDate = this.StartDate };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets EndDate.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets Question.
        /// </summary>
        public TimelineQuestion Question { get; set; }

        /// <summary>
        /// Gets or sets StartDate.
        /// </summary>
        public DateTime StartDate { get; set; }

        #endregion
    }
}