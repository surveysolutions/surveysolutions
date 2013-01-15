// -----------------------------------------------------------------------
// <copyright file="SurveyAnswer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Answer;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SurveyAnswer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyAnswer"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        public SurveyAnswer(ICompleteQuestion question)
        {
            this.IsReadOnly = question.QuestionScope != QuestionScope.Supervisor;
            this.Enabled = question.Enabled;
            this.Answers = question.Answers.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(question.PublicKey, a)).ToArray();
            this.Answer = question.GetAnswerString();
            this.Valid = question.Valid;
            this.Answered = question.IsAnswered();
            this.Comments = question.Comments;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets Answers.
        /// </summary>
        public CompleteAnswerView[] Answers { get; set; }

        /// <summary>
        /// Gets or sets Answer.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Valid.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Answered.
        /// </summary>
        public bool Answered { get; set; }

        /// <summary>
        /// Gets or sets Comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsReadOnly.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
