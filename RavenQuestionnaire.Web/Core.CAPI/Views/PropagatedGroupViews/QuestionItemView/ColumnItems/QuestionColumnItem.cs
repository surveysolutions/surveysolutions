// -----------------------------------------------------------------------
// <copyright file="QuestionColumnItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Answer;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView.ColumnItems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionColumnItem : PropagatedGroupColumnItem
    {
        public QuestionColumnItem(ICompleteQuestion question)
        {
            this.Title = question.QuestionText;
            this.ItemPublicKey = question.PublicKey;
            this.Answer = question.GetAnswerObject();
            this.Enabled = question.Enabled;
            this.Valid = question.Valid;
            this.ValidationMessage = question.ValidationMessage;
            this.Comments = question.Comments;
            this.Instructions = question.Instructions;
            this.Mandatory = question.Mandatory;
            this.QuestionType = question.QuestionType;
            this.Answers =
                question.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(question.PublicKey, a)).ToList();
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
        /// Gets or sets the answer.
        /// </summary>
        public object Answer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether answered.
        /// </summary>
        public bool Answered
        {
            get { return this.Answer == null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Gets or sets the validation message.
        /// </summary>
        public string ValidationMessage { get; set; }
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }
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
        #endregion
    }
}
