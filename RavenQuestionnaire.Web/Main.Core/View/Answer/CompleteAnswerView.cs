using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.View.Answer
{
    /// <summary>
    /// The complete answer view.
    /// </summary>
    public class CompleteAnswerView : AnswerView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswerView"/> class.
        /// </summary>
        public CompleteAnswerView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswerView"/> class.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteAnswerView(Guid questionPublicKey, IAnswer doc)
            : base(questionPublicKey, doc)
        {
            this.Selected = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteAnswerView"/> class.
        /// </summary>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="answer">
        /// The answer.
        /// </param>
        public CompleteAnswerView(Guid questionKey, ICompleteAnswer answer)
            : base(questionKey, answer)
        {
            this.Selected = answer.Selected;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether selected.
        /// </summary>
        public bool Selected { get; set; }

        #endregion
    }
}