namespace Main.Core.View.Question
{
    using System.Collections.Generic;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.Answer;

    /// <summary>
    /// MultyOptions view
    /// </summary>
    public class MultyOptionsQuestionView : BaseQuestionView
    {
        #region Fields

        /// <summary>
        /// The _answers.
        /// </summary>
        private AnswerView[] _answers;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MultyOptionsQuestionView"/> class.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        public MultyOptionsQuestionView(QuestionView view) : base(view)
        {
            this.AnswerOrder = view.AnswerOrder;
            this.Answers = view.Answers;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the answer order.
        /// </summary>
        public Order AnswerOrder { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        public AnswerView[] Answers
        {
            get
            {
                return this._answers;
            }

            set
            {
                this._answers = value;
                if (this._answers == null)
                {
                    this._answers = new AnswerView[0];
                    return;
                }

                for (int i = 0; i < this._answers.Length; i++)
                {
                    this._answers[i].Index = i + 1;
                }
            }
        }

        #endregion
    }
}
