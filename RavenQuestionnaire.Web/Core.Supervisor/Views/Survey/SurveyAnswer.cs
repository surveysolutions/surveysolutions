namespace Core.Supervisor.Views.Survey
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
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
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        public SurveyAnswer(ICompleteGroup group, ICompleteQuestion question)
        {
            this.Key = new ScreenKey(question.PublicKey, question.PropagationPublicKey, group.Propagated);
            this.ParentKey = new ScreenKey(group);
            
            this.IsReadOnly = question.QuestionScope != QuestionScope.Supervisor;
            this.Enabled = question.Enabled;
            this.Valid = question.Valid;
            this.Answered = question.IsAnswered();
            this.IsFlaged = question.IsFlaged;

            this.AnswerOptions = question.Answers.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(question.PublicKey, a)).ToArray();
            this.Type = question.QuestionType;
            this.Answer = question.GetAnswerString();
           
            this.Comments = question.Comments;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets Answers.
        /// </summary>
        public CompleteAnswerView[] AnswerOptions { get; set; }

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
        /// Gets or sets a value indicating whether IsFlaged.
        /// </summary>
        public bool IsFlaged { get; set; }

        /// <summary>
        /// Gets or sets Comments.
        /// </summary>
        public List<CommentDocument> Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsReadOnly.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets Key.
        /// </summary>
        public ScreenKey Key { get; set; }

        /// <summary>
        /// Gets or sets ParentKey.
        /// </summary>
        public ScreenKey ParentKey { get; set; }

        /// <summary>
        /// Gets or sets Type.
        /// </summary>
        public QuestionType Type { get; set; }
    }
}
