namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Answer;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionColumnItem : PropagatedGroupColumnItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionColumnItem"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="parentGroupPublicKey">
        /// The parent group public key.
        /// </param>
        public QuestionColumnItem(ICompleteQuestion question, Guid questionnaireId, Guid parentGroupPublicKey)
        {
            this.Title = question.QuestionText;
            this.ItemPublicKey = question.PublicKey;
            this.ValidationMessage = question.ValidationMessage;
            this.Instructions = question.Instructions;
            this.Mandatory = question.Mandatory;
            this.QuestionType = question.QuestionType;
            this.QuestionnaireId = questionnaireId;
            this.ParentGroupPublicKey = parentGroupPublicKey;
            this.Answers =
                question.Answers.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(question.PublicKey, a)).ToList();
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the answers.
        /// </summary>
        public List<CompleteAnswerView> Answers
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
                    this._answers = new List<CompleteAnswerView>();
                    return;
                }

                for (int i = 0; i < this._answers.Count; i++)
                {
                    this._answers[i].Index = i + 1;
                }
            }
        }
        private List<CompleteAnswerView> _answers;

      
        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }
      
        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }
        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        public Guid QuestionnaireId { get; set; }
        public Guid ParentGroupPublicKey { get; set; }

        #endregion
    }
}
