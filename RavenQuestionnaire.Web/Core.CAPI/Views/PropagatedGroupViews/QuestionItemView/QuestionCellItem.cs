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
            if (!question.PropagationPublicKey.HasValue)
                throw new ArgumentException("question have to be propagated");
            PublicKey = question.PublicKey;
            PropagationKey = question.PropagationPublicKey.Value;
            Answer = question.GetAnswerObject();
            AnswerString = question.GetAnswerString();
            var answerKeys = question.Answers.OfType<ICompleteAnswer>().Where(a => a.Selected).Select(a=>a.PublicKey.ToString());
            if (answerKeys.Any())
                AnswerPublicKeys = string.Join(";", answerKeys);
            Enabled = question.Enabled;
            Valid = question.Valid;
            Comments = question.LastComment;
        }

        public Guid PublicKey { get; set; }
        public Guid PropagationKey { get; set; }
        public object Answer { get; set; }
        public string AnswerString { get; set; }
        public string AnswerPublicKeys { get; set; }
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
            get { return this.Answer != null; }
        }

    }
}
