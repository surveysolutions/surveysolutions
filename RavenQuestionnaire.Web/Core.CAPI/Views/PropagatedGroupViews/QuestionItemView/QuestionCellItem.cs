// -----------------------------------------------------------------------
// <copyright file="QuestionCellItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;

namespace Core.CAPI.Views.PropagatedGroupViews.QuestionItemView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionCellItem
    {
        public QuestionCellItem(ICompleteQuestion question)
        {
            if (!question.PropogationPublicKey.HasValue)
                throw new ArgumentException("question have to be propagated");
            PublicKey = question.PublicKey;
            PropagationKey = question.PropogationPublicKey.Value;
            Answer = question.GetAnswerObject();
            AnswerString = question.GetAnswerString();
            var firstAnswer = question.Children.OfType<ICompleteAnswer>().FirstOrDefault(a => a.Selected);
            if (firstAnswer != null)
                AnswerPublicKey = firstAnswer.PublicKey;
            Enabled = question.Enabled;
            Valid = question.Valid;
            Comments = question.Comments;
        }

        public Guid PublicKey { get; set; }
        public Guid PropagationKey { get; set; }
        public object Answer { get; set; }
        public string AnswerString { get; set; }
        public Guid AnswerPublicKey { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid { get; set; }
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether answered.
        /// </summary>
        public bool Answered
        {
            get { return this.Answer == null; }
        }

    }
}
